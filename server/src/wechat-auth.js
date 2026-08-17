const DEFAULT_CODE2SESSION_URL = 'https://api.weixin.qq.com/sns/jscode2session';

async function exchangeWechatCode(code, devOpenId) {
  const isDevMode = process.env.DEV_MODE !== 'false';
  if (isDevMode && (code === 'mock-code-editor' || devOpenId)) {
    return {
      openId: devOpenId || 'mock-openid',
      unionId: '',
      sessionKey: 'mock-session-key'
    };
  }

  if (!code) throw new Error('缺少微信登录 code');
  const appId = process.env.WECHAT_APPID;
  const appSecret = process.env.WECHAT_APP_SECRET;
  if (!appId || !appSecret) {
    throw new Error('服务端未配置 WECHAT_APPID / WECHAT_APP_SECRET');
  }

  const endpoint = process.env.WECHAT_CODE2SESSION_URL || DEFAULT_CODE2SESSION_URL;
  const url = new URL(endpoint);
  url.searchParams.set('appid', appId);
  url.searchParams.set('secret', appSecret);
  url.searchParams.set('js_code', code);
  url.searchParams.set('grant_type', 'authorization_code');

  const response = await fetch(url);
  if (!response.ok) throw new Error(`微信 code2Session HTTP ${response.status}`);
  const result = await response.json();
  if (!result.openid) {
    throw new Error(`微信 code2Session 失败：${result.errmsg || result.errcode || '未知错误'}`);
  }
  return {
    openId: result.openid,
    unionId: result.unionid || '',
    sessionKey: result.session_key || ''
  };
}

module.exports = { exchangeWechatCode };
