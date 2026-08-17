const fs = require('fs');
const path = require('path');

const STATE_FILE = process.env.STATE_FILE || path.join(__dirname, '..', 'data', 'state.json');

const FRIENDS = [
  {
    friendId: 'friend-jia',
    displayName: '牢甲',
    traitIds: ['slacker', 'lucky'],
    traitNames: ['摸鱼王', '锦鲤'],
    productionPerHour: 14,
    escapeChance: 8,
    assignedLayer: 1,
    avatarColor: '#D8835B',
    catchLine: '别装忙，我都看见你在群里发红包了。'
  },
  {
    friendId: 'friend-xin',
    displayName: '牢昕',
    traitIds: ['night-owl', 'boss'],
    traitNames: ['夜猫子', '老板气质'],
    productionPerHour: 18,
    escapeChance: 12,
    assignedLayer: 2,
    avatarColor: '#8C78C8',
    catchLine: '她说今晚不睡，结果先把矿灯关了。'
  },
  {
    friendId: 'friend-yang',
    displayName: '老杨',
    traitIds: ['workaholic', 'lucky'],
    traitNames: ['卷王', '锦鲤'],
    productionPerHour: 22,
    escapeChance: 5,
    assignedLayer: 2,
    avatarColor: '#E4AF57',
    catchLine: '他刚进来就问：KPI 是按吨算还是按命算？'
  }
];

function nowSeconds() {
  return Math.floor(Date.now() / 1000);
}

function createDefaultPlayer(playerId) {
  return {
    playerId,
    coins: 328,
    ore: 76,
    crystals: 2,
    treasures: 1,
    depthMeters: 28,
    unlockedLayerCount: 2,
    lastSettlementUnixSeconds: nowSeconds() - 38 * 60,
    selectedFriendId: '',
    miners: [
      {
        friendId: 'mock-miner-1',
        displayName: '小锤',
        traitNames: ['卷王', '夜猫子'],
        assignedLayer: 1,
        gridColumn: 2,
        gridRow: 3,
        status: '正在猛挖',
        miningPower: 8,
        discoveryBonus: 2
      },
      {
        friendId: 'mock-miner-2',
        displayName: '胖虎',
        traitNames: ['老板气质'],
        assignedLayer: 1,
        gridColumn: 7,
        gridRow: 4,
        status: '研究矿脉',
        miningPower: 6,
        discoveryBonus: 5
      }
    ]
  };
}

function createEmptyState() {
  return { players: {} };
}

function readState() {
  try {
    if (fs.existsSync(STATE_FILE)) {
      const parsed = JSON.parse(fs.readFileSync(STATE_FILE, 'utf8'));
      if (parsed && parsed.players) return parsed;
    }
  } catch (error) {
    console.warn(`[store] 读取状态失败，将使用空状态：${error.message}`);
  }
  return createEmptyState();
}

let state = readState();

function persist() {
  const directory = path.dirname(STATE_FILE);
  fs.mkdirSync(directory, { recursive: true });
  const tempFile = `${STATE_FILE}.tmp`;
  fs.writeFileSync(tempFile, JSON.stringify(state, null, 2), 'utf8');
  fs.renameSync(tempFile, STATE_FILE);
}

function ensurePlayer(playerId) {
  if (!playerId) throw new Error('缺少 playerId');
  if (!state.players[playerId]) {
    state.players[playerId] = createDefaultPlayer(playerId);
    persist();
  }
  return state.players[playerId];
}

function clone(value) {
  return JSON.parse(JSON.stringify(value));
}

function getPlayerPower(player) {
  return 4 + player.miners.reduce((sum, miner) => sum + Math.max(1, miner.miningPower || 1), 0);
}

function getDiscoveryPower(player) {
  return 1 + player.miners.reduce((sum, miner) => sum + Math.max(0, miner.discoveryBonus || 0), 0);
}

function getFriendsForPlayer(player) {
  const recruited = new Set(player.miners.map((miner) => miner.friendId));
  return FRIENDS.map((friend) => ({
    ...friend,
    status: recruited.has(friend.friendId) ? 'recruited' : 'waiting'
  }));
}

function getBundle(playerId, offline = emptyReward()) {
  const player = ensurePlayer(playerId);
  return {
    player: clone(player),
    friends: getFriendsForPlayer(player),
    offline
  };
}

function emptyReward() {
  return { coins: 0, ore: 0, crystals: 0, treasures: 0, elapsedMinutes: 0 };
}

function settleOffline(playerId, requestedNow) {
  const player = ensurePlayer(playerId);
  const serverNow = nowSeconds();
  const safeNow = Math.min(Number(requestedNow) || serverNow, serverNow + 60);
  const last = Number(player.lastSettlementUnixSeconds) || safeNow;
  const elapsedMinutes = Math.max(0, Math.min(8 * 60, Math.floor((safeNow - last) / 60)));
  const reward = emptyReward();
  reward.elapsedMinutes = elapsedMinutes;
  if (elapsedMinutes > 0) {
    const hourlyPower = getPlayerPower(player);
    const hourlyDiscovery = getDiscoveryPower(player);
    reward.coins = Math.max(1, Math.round(hourlyPower * elapsedMinutes / 60));
    reward.ore = Math.max(1, Math.round((4 + player.miners.length * 2) * elapsedMinutes / 60));
    reward.crystals = Math.max(0, Math.floor((hourlyDiscovery + elapsedMinutes / 60) / 10));
    reward.treasures = Math.max(0, Math.floor((hourlyDiscovery + elapsedMinutes / 60) / 40));
    player.coins += reward.coins;
    player.ore += reward.ore;
    player.crystals += reward.crystals;
    player.treasures += reward.treasures;
  }
  player.lastSettlementUnixSeconds = safeNow;
  persist();
  return getBundle(playerId, reward);
}

function recruitFriend(playerId, friendId) {
  const player = ensurePlayer(playerId);
  const friend = FRIENDS.find((candidate) => candidate.friendId === friendId);
  if (!friend) throw new Error('好友不在 Mock 名单中');
  if (!player.miners.some((miner) => miner.friendId === friendId)) {
    const index = player.miners.length;
    const power = friend.traitIds.reduce((sum, traitId) => sum + traitPower(traitId), 0);
    const discovery = friend.traitIds.reduce((sum, traitId) => sum + traitDiscovery(traitId), 0);
    player.miners.push({
      friendId: friend.friendId,
      displayName: friend.displayName,
      traitNames: friend.traitNames,
      assignedLayer: friend.assignedLayer,
      gridColumn: 1 + (index * 3) % 9,
      gridRow: 3 + (index * 2) % 8,
      status: '刚被抓来，正在适应镐头',
      miningPower: Math.max(1, 7 + power),
      discoveryBonus: Math.max(0, discovery)
    });
  }
  player.selectedFriendId = friendId;
  persist();
  return getBundle(playerId);
}

function traitPower(traitId) {
  return { slacker: -2, workaholic: 5, 'night-owl': 2, boss: 1 }[traitId] || 0;
}

function traitDiscovery(traitId) {
  return { lucky: 8, 'night-owl': 3, boss: 6 }[traitId] || 0;
}

module.exports = {
  getBundle,
  settleOffline,
  recruitFriend
};
