namespace AliceInCradleHack.module.modules.client.webui
{
    /// <summary>
    /// Embedded single-file WebUI page (dark theme, Chinese, no external resources).
    /// </summary>
    public static class WebUiPage
    {
        public const string Html = @"<!DOCTYPE html>
<html lang=""zh-CN"">
<head>
<meta charset=""utf-8"">
<meta name=""viewport"" content=""width=device-width, initial-scale=1"">
<title>AliceInCradle Hack</title>
<style>
:root {
  --bg: #0f1115;
  --panel: #171a21;
  --panel2: #1e222b;
  --border: #2a2f3a;
  --text: #e6e9ef;
  --muted: #8a91a0;
  --accent: #5aa9ff;
  --on: #3fb950;
  --danger: #f85149;
}
* { box-sizing: border-box; margin: 0; padding: 0; }
body {
  background: var(--bg); color: var(--text);
  font-family: ""Segoe UI"", ""Microsoft YaHei"", sans-serif;
  padding: 24px; max-width: 900px; margin: 0 auto;
}
header { display: flex; align-items: center; justify-content: space-between; margin-bottom: 20px; }
h1 { font-size: 20px; font-weight: 600; }
h1 .sub { color: var(--muted); font-size: 13px; font-weight: 400; margin-left: 8px; }
.btn {
  background: var(--panel2); color: var(--text); border: 1px solid var(--border);
  border-radius: 6px; padding: 6px 14px; cursor: pointer; font-size: 13px;
}
.btn:hover { border-color: var(--accent); }
.category { color: var(--accent); font-size: 14px; margin: 18px 0 8px; }
.card {
  background: var(--panel); border: 1px solid var(--border); border-radius: 8px;
  margin-bottom: 10px; overflow: hidden;
}
.card-head { display: flex; align-items: center; padding: 12px 16px; cursor: pointer; user-select: none; }
.card-head:hover { background: var(--panel2); }
.mod-info { flex: 1; min-width: 0; }
.mod-name { font-size: 15px; font-weight: 600; }
.mod-name .badge { font-size: 11px; color: var(--muted); font-weight: 400; margin-left: 8px; }
.mod-desc { color: var(--muted); font-size: 12px; margin-top: 3px; }
.chevron { color: var(--muted); margin-left: 12px; transition: transform .15s; font-size: 12px; }
.card.open .chevron { transform: rotate(90deg); }
.switch { position: relative; width: 42px; height: 22px; flex-shrink: 0; margin-left: 12px; }
.switch input { opacity: 0; width: 100%; height: 100%; position: absolute; cursor: pointer; z-index: 2; margin: 0; }
.switch .track {
  position: absolute; inset: 0; border-radius: 11px; background: var(--panel2);
  border: 1px solid var(--border); transition: background .15s;
}
.switch .thumb {
  position: absolute; top: 3px; left: 3px; width: 16px; height: 16px; border-radius: 50%;
  background: var(--muted); transition: all .15s;
}
.switch input:checked ~ .track { background: rgba(63,185,80,.25); border-color: var(--on); }
.switch input:checked ~ .thumb { left: 23px; background: var(--on); }
.switch input:disabled { cursor: not-allowed; }
.switch input:disabled ~ .track { opacity: .5; }
.settings { border-top: 1px solid var(--border); display: none; }
.card.open .settings { display: block; }
.setting { display: flex; align-items: center; padding: 10px 16px; border-bottom: 1px solid var(--border); }
.setting:last-child { border-bottom: none; }
.setting-info { flex: 1; min-width: 0; }
.setting-name { font-size: 13px; }
.setting-desc { color: var(--muted); font-size: 11px; margin-top: 2px; }
.setting-control { margin-left: 16px; flex-shrink: 0; }
.setting-control input[type=text], .setting-control input[type=number] {
  background: var(--panel2); border: 1px solid var(--border); border-radius: 5px;
  color: var(--text); padding: 5px 9px; font-size: 13px; width: 220px;
}
.setting-control input[type=number] { width: 120px; }
.setting-control input:focus { outline: none; border-color: var(--accent); }
.setting-control input:disabled { opacity: .5; }
.readonly-tag { color: var(--muted); font-size: 12px; }
.empty { color: var(--muted); padding: 12px 16px; font-size: 13px; }
#toast {
  position: fixed; bottom: 24px; left: 50%; transform: translateX(-50%);
  background: var(--panel2); border: 1px solid var(--border); border-radius: 6px;
  padding: 10px 18px; font-size: 13px; opacity: 0; transition: opacity .2s; pointer-events: none;
}
#toast.show { opacity: 1; }
#toast.err { border-color: var(--danger); color: var(--danger); }
</style>
</head>
<body>
<header>
  <h1>AliceInCradle Hack<span class=""sub"">WebUI</span></h1>
  <button class=""btn"" onclick=""loadModules()"">刷新</button>
</header>
<div id=""content""><div class=""empty"">加载中…</div></div>
<div id=""toast""></div>
<script>
const content = document.getElementById('content');
const toastEl = document.getElementById('toast');
let toastTimer = null;

function toast(msg, isErr) {
  toastEl.textContent = msg;
  toastEl.className = 'show' + (isErr ? ' err' : '');
  clearTimeout(toastTimer);
  toastTimer = setTimeout(() => toastEl.className = '', 2200);
}

async function api(path, options) {
  const res = await fetch(path, options);
  const data = await res.json().catch(() => ({}));
  if (!res.ok) throw new Error(data.error || ('HTTP ' + res.status));
  return data;
}

async function loadModules() {
  let modules;
  try {
    modules = await api('/api/modules');
  } catch (e) {
    content.innerHTML = '<div class=""empty"">加载失败: ' + esc(e.message) + '</div>';
    return;
  }
  const groups = {};
  for (const m of modules) (groups[m.category] = groups[m.category] || []).push(m);
  content.innerHTML = '';
  for (const cat of Object.keys(groups)) {
    const h = document.createElement('div');
    h.className = 'category';
    h.textContent = cat;
    content.appendChild(h);
    for (const m of groups[cat]) content.appendChild(renderModule(m));
  }
}

function renderModule(m) {
  const card = document.createElement('div');
  card.className = 'card';
  card.dataset.name = m.name;

  const head = document.createElement('div');
  head.className = 'card-head';
  head.innerHTML =
    '<div class=""mod-info"">' +
      '<div class=""mod-name"">' + esc(m.name) +
        '<span class=""badge"">' + esc(m.version) + ' · ' + esc(m.author) + '</span>' +
      '</div>' +
      '<div class=""mod-desc"">' + esc(m.description || '') + '</div>' +
    '</div>' +
    '<label class=""switch"" onclick=""event.stopPropagation()"">' +
      '<input type=""checkbox"" ' + (m.isEnabled ? 'checked ' : '') + (m.isSelf ? 'disabled title=""不能从网页关闭 WebUI 自身"" ' : '') + '>' +
      '<span class=""track""></span><span class=""thumb""></span>' +
    '</label>' +
    '<span class=""chevron"">▶</span>';
  card.appendChild(head);

  const settings = document.createElement('div');
  settings.className = 'settings';
  card.appendChild(settings);

  head.querySelector('input').addEventListener('change', async ev => {
    const cb = ev.target;
    cb.disabled = true;
    try {
      const r = await api('/api/modules/' + encodeURIComponent(m.name) + '/toggle', { method: 'POST' });
      cb.checked = r.isEnabled;
      toast(m.name + (r.isEnabled ? ' 已开启' : ' 已关闭'));
    } catch (e) {
      cb.checked = !cb.checked;
      toast(e.message, true);
    }
    cb.disabled = m.isSelf;
  });

  head.addEventListener('click', () => {
    card.classList.toggle('open');
    if (card.classList.contains('open') && !settings.dataset.loaded) {
      settings.dataset.loaded = '1';
      loadSettings(m.name, settings);
    }
  });

  return card;
}

async function loadSettings(name, container) {
  container.innerHTML = '<div class=""empty"">加载中…</div>';
  let list;
  try {
    list = await api('/api/modules/' + encodeURIComponent(name) + '/settings');
  } catch (e) {
    container.innerHTML = '<div class=""empty"">加载失败: ' + esc(e.message) + '</div>';
    return;
  }
  container.innerHTML = '';
  if (!list.length) {
    container.innerHTML = '<div class=""empty"">该模块没有可配置项</div>';
    return;
  }
  for (const s of list) container.appendChild(renderSetting(name, s));
}

function renderSetting(moduleName, s) {
  const row = document.createElement('div');
  row.className = 'setting';
  const info = document.createElement('div');
  info.className = 'setting-info';
  info.innerHTML =
    '<div class=""setting-name"">' + esc(s.path) + '</div>' +
    '<div class=""setting-desc"">' + esc(s.description || '') + '</div>';
  row.appendChild(info);

  const ctrl = document.createElement('div');
  ctrl.className = 'setting-control';

  if (!s.isEditable) {
    ctrl.innerHTML = '<span class=""readonly-tag"">' + esc(String(s.value)) + ' (只读)</span>';
  } else if (s.type === 'Boolean') {
    const cb = document.createElement('input');
    cb.type = 'checkbox';
    cb.checked = !!s.value;
    cb.addEventListener('change', () => saveSetting(moduleName, s.path, cb.checked));
    ctrl.appendChild(cb);
  } else if (s.type === 'Int32' || s.type === 'Int64' || s.type === 'Double' || s.type === 'Single' || s.type === 'Float') {
    const inp = document.createElement('input');
    inp.type = 'number';
    inp.step = 'any';
    inp.value = s.value;
    inp.addEventListener('change', () => saveSetting(moduleName, s.path, Number(inp.value)));
    ctrl.appendChild(inp);
  } else {
    const inp = document.createElement('input');
    inp.type = 'text';
    inp.value = s.value == null ? '' : String(s.value);
    inp.addEventListener('change', () => saveSetting(moduleName, s.path, inp.value));
    ctrl.appendChild(inp);
  }

  row.appendChild(ctrl);
  return row;
}

async function saveSetting(moduleName, path, value) {
  try {
    await api('/api/modules/' + encodeURIComponent(moduleName) + '/settings', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ path, value })
    });
    toast('已保存 ' + path);
  } catch (e) {
    toast(e.message, true);
  }
}

function esc(s) {
  return String(s).replace(/[&<>""]/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '""': '&quot;' }[c]));
}

loadModules();
</script>
</body>
</html>";
    }
}
