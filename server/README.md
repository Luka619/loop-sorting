# 抓好友下矿：原型服务器

这是第一版的无依赖 Node.js 服务器，使用 `server/data/state.json` 保存 Mock 玩家状态，方便先把 Unity 原型和游戏循环跑通。后续可以把 `src/store.js` 替换成现有服务器的数据库实现，接口保持不变。

## 启动

需要 Node.js 18 或更高版本：

```powershell
cd D:\A-workspace\loop-sorting\server
npm start
```

默认监听 `http://localhost:8787`。

## 从 GitHub 下载

仓库为私有仓库，先登录 GitHub，再执行：

```powershell
git clone https://github.com/Luka619/loop-sorting.git
cd loop-sorting/server
npm start
```

如果下载的是其他分支，在 `git clone` 后切换到对应分支即可。

快速检查：

```powershell
Invoke-RestMethod http://localhost:8787/api/health
Invoke-RestMethod 'http://localhost:8787/api/player/state?playerId=mock-openid'
```

## 接口

- `GET /api/health`
- `POST /api/auth/wechat`：小游戏把 `wx.login` 得到的临时 `code` 发到这里；服务器再调用微信 `code2Session`，客户端不接触 `appSecret`、`session_key`。
- `GET /api/player/state?playerId=...`
- `GET /api/friends?playerId=...`
- `POST /api/friends/:friendId/recruit`，body 为 `{ "playerId": "..." }`
- `POST /api/mining/settle`，body 为 `{ "playerId": "...", "nowUnixSeconds": 0 }`

## 环境变量

原型默认 `DEV_MODE=true`，可以用 `mock-code-editor` 或 `devOpenId` 开发。接入真实微信前配置：

```text
PORT=8787
DEV_MODE=false
WECHAT_APPID=你的小程序或小游戏 AppID
WECHAT_APP_SECRET=只放在服务器上的 AppSecret
ALLOWED_ORIGIN=https://你的业务域名
```

不要把 `WECHAT_APP_SECRET` 写入 Unity 工程、小游戏资源包或客户端配置。微信好友也不是客户端可以直接读取的完整联系人列表；真实好友关系建议通过分享、群/单聊入口、明确授权后的好友行为和服务端关系记录逐步建立。当前三名好友是 Mock 数据，后续只替换 `src/store.js` 的数据来源。
