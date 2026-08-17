const http = require('http');
const crypto = require('crypto');
const { exchangeWechatCode } = require('./wechat-auth');
const store = require('./store');

const PORT = Number(process.env.PORT || 8787);
const ALLOWED_ORIGIN = process.env.ALLOWED_ORIGIN || '*';
const sessions = new Map();

function json(res, statusCode, payload) {
  const body = JSON.stringify(payload);
  res.writeHead(statusCode, {
    'Content-Type': 'application/json; charset=utf-8',
    'Content-Length': Buffer.byteLength(body),
    'Access-Control-Allow-Origin': ALLOWED_ORIGIN,
    'Access-Control-Allow-Headers': 'Content-Type, Authorization',
    'Access-Control-Allow-Methods': 'GET, POST, OPTIONS'
  });
  res.end(body);
}

function error(res, statusCode, message) {
  json(res, statusCode, { error: message });
}

function readJson(req) {
  return new Promise((resolve, reject) => {
    let body = '';
    req.on('data', (chunk) => {
      body += chunk;
      if (body.length > 1024 * 1024) reject(new Error('请求体过大'));
    });
    req.on('end', () => {
      if (!body) return resolve({});
      try {
        resolve(JSON.parse(body));
      } catch (_) {
        reject(new Error('请求 JSON 无法解析'));
      }
    });
    req.on('error', reject);
  });
}

function getSessionPlayerId(req) {
  const header = req.headers.authorization || '';
  const token = header.startsWith('Bearer ') ? header.slice(7) : '';
  return token ? sessions.get(token) : '';
}

function resolvePlayerId(req, url, body) {
  const sessionPlayerId = getSessionPlayerId(req);
  if (sessionPlayerId) return sessionPlayerId;
  if (process.env.DEV_MODE !== 'false') {
    return body.playerId || url.searchParams.get('playerId') || 'mock-openid';
  }
  return '';
}

async function route(req, res) {
  if (req.method === 'OPTIONS') return json(res, 204, {});
  const url = new URL(req.url, `http://${req.headers.host || 'localhost'}`);
  const body = req.method === 'POST' ? await readJson(req) : {};

  if (req.method === 'GET' && url.pathname === '/api/health') {
    return json(res, 200, { ok: true, service: 'loop-sorting-mining-server', nowUnixSeconds: Math.floor(Date.now() / 1000) });
  }

  if (req.method === 'POST' && url.pathname === '/api/auth/wechat') {
    const auth = await exchangeWechatCode(body.code, body.devOpenId);
    const token = crypto.randomBytes(24).toString('hex');
    sessions.set(token, auth.openId);
    return json(res, 200, { sessionToken: token, playerId: auth.openId, openId: auth.openId });
  }

  const playerId = resolvePlayerId(req, url, body);
  if (!playerId) return error(res, 401, '未登录或登录态已失效');

  if (req.method === 'GET' && url.pathname === '/api/player/state') {
    return json(res, 200, store.getBundle(playerId));
  }

  if (req.method === 'GET' && url.pathname === '/api/friends') {
    return json(res, 200, { friends: store.getBundle(playerId).friends });
  }

  const recruitMatch = url.pathname.match(/^\/api\/friends\/([^/]+)\/recruit$/);
  if (req.method === 'POST' && recruitMatch) {
    return json(res, 200, store.recruitFriend(playerId, decodeURIComponent(recruitMatch[1])));
  }

  if (req.method === 'POST' && url.pathname === '/api/mining/settle') {
    return json(res, 200, store.settleOffline(playerId, body.nowUnixSeconds));
  }

  return error(res, 404, '接口不存在');
}

const server = http.createServer((req, res) => {
  route(req, res).catch((routeError) => {
    console.error(routeError);
    error(res, 400, routeError.message || '请求处理失败');
  });
});

server.listen(PORT, () => {
  console.log(`[loop-sorting] mining server listening on http://localhost:${PORT}`);
  console.log(`[loop-sorting] DEV_MODE=${process.env.DEV_MODE === 'false' ? 'false' : 'true'}`);
});
