namespace AliceInCradleHack.module.modules.client.webui
{
    /// <summary>
    /// Embedded single-file WebUI page (dark theme, English, no external resources).
    /// </summary>
    public static class WebUiPage
    {
        public const string Html = @"<!DOCTYPE html>
<html lang=""en"">
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
header { display: flex; align-items: center; justify-content: space-between; margin-bottom: 12px; flex-wrap: wrap; gap: 8px; }
h1 { font-size: 20px; font-weight: 600; }
h1 .sub { color: var(--muted); font-size: 13px; font-weight: 400; margin-left: 8px; }
.btn {
  background: var(--panel2); color: var(--text); border: 1px solid var(--border);
  border-radius: 6px; padding: 6px 12px; cursor: pointer; font-size: 13px;
}
.btn:hover { border-color: var(--accent); }
.config-bar {
  display: flex; align-items: center; gap: 8px; flex-wrap: wrap;
  background: var(--panel); border: 1px solid var(--border); border-radius: 8px;
  padding: 10px 14px; margin-bottom: 6px;
}
.config-bar label { color: var(--muted); font-size: 13px; }
.config-bar select {
  background: var(--panel2); color: var(--text); border: 1px solid var(--border);
  border-radius: 5px; padding: 5px 8px; font-size: 13px; flex: 1; min-width: 140px;
}
.config-bar select:focus { outline: none; border-color: var(--accent); }
.category { color: var(--accent); font-size: 14px; margin: 18px 0 8px; }
.card {
  background: var(--panel); border: 1px solid var(--border); border-radius: 8px;
  margin-bottom: 10px; overflow: hidden;
}
.card-head { display: flex; align-items: center; padding: 12px 16px; cursor: pointer; user-select: none; }
.card-head:hover { background: var(--panel2); }
.mod-info { flex: 1; min-width: 0; }
.mod-name { font-size: 15px; font-weight: 600; }
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
.setting-control { margin-left: 16px; flex-shrink: 0; display: flex; align-items: center; gap: 8px; }
.setting-control input[type=text], .setting-control input[type=number], .setting-control select {
  background: var(--panel2); border: 1px solid var(--border); border-radius: 5px;
  color: var(--text); padding: 5px 9px; font-size: 13px; width: 220px;
}
.setting-control input[type=number] { width: 100px; }
.setting-control input[type=range] { width: 160px; accent-color: var(--accent); cursor: pointer; }
.setting-control input[type=color] {
  width: 42px; height: 30px; padding: 2px; border: 1px solid var(--border);
  border-radius: 5px; background: var(--panel2); cursor: pointer;
}
.setting-control input:focus, .setting-control select:focus { outline: none; border-color: var(--accent); }
.setting-control input:disabled { opacity: .5; }
.setting-control .suffix { color: var(--muted); font-size: 12px; min-width: 18px; }
.readonly-tag { color: var(--muted); font-size: 12px; }
.empty { color: var(--muted); padding: 12px 16px; font-size: 13px; }
#toast {
  position: fixed; bottom: 24px; left: 50%; transform: translateX(-50%);
  background: var(--panel2); border: 1px solid var(--border); border-radius: 6px;
  padding: 10px 18px; font-size: 13px; opacity: 0; transition: opacity .2s; pointer-events: none;
  z-index: 10; max-width: 90vw;
}
#toast.show { opacity: 1; }
#toast.err { border-color: var(--danger); color: var(--danger); }
</style>
</head>
<body>
<header>
  <h1>AliceInCradle Hack<span class=""sub"">WebUI</span></h1>
  <div style=""display:flex;gap:8px;flex-wrap:wrap;"">
    <button class=""btn"" onclick=""loadModules()"">Refresh</button>
    <button class=""btn"" onclick=""exportConfig()"">Export</button>
    <button class=""btn"" onclick=""document.getElementById('importFile').click()"">Import</button>
    <button class=""btn"" onclick=""saveConfig()"">Save</button>
  </div>
</header>
<div class=""config-bar"">
  <label for=""savedConfigs"">Load:</label>
  <select id=""savedConfigs""><option value="""">(none saved)</option></select>
  <button class=""btn"" onclick=""loadConfig()"">Load</button>
</div>
<input type=""file"" id=""importFile"" accept="".json,application/json"" style=""display:none"">
<div id=""content""><div class=""empty"">Loading…</div></div>
<div id=""toast""></div>
<script>
const content = document.getElementById('content');
const toastEl = document.getElementById('toast');
const savedSelect = document.getElementById('savedConfigs');
const importFile = document.getElementById('importFile');
let toastTimer = null;
const saveTimers = {};

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

function debounce(key, fn, ms) {
  clearTimeout(saveTimers[key]);
  saveTimers[key] = setTimeout(fn, ms);
}

async function loadModules() {
  let modules;
  try {
    modules = await api('/api/modules');
  } catch (e) {
    content.innerHTML = '<div class=""empty"">Failed to load modules: ' + esc(e.message) + '</div>';
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
      '</div>' +
      '<div class=""mod-desc"">' + esc(m.description || '') + '</div>' +
    '</div>' +
    '<label class=""switch"" onclick=""event.stopPropagation()"">' +
      '<input type=""checkbox"" ' + (m.isEnabled ? 'checked ' : '') + (m.isSelf ? 'disabled title=""Cannot disable WebUI itself"" ' : '') + '>' +
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
      toast(m.name + (r.isEnabled ? ' enabled' : ' disabled'));
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
  container.innerHTML = '<div class=""empty"">Loading…</div>';
  let list;
  try {
    list = await api('/api/modules/' + encodeURIComponent(name) + '/settings');
  } catch (e) {
    container.innerHTML = '<div class=""empty"">Failed to load: ' + esc(e.message) + '</div>';
    return;
  }
  container.innerHTML = '';
  if (!list.length) {
    container.innerHTML = '<div class=""empty"">This module has no configurable options.</div>';
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
  row.appendChild(ctrl);

  if (!s.isEditable) {
    ctrl.innerHTML = '<span class=""readonly-tag"">' + esc(String(s.value)) + ' (readonly)</span>';
    return row;
  }

  switch (s.type) {
    case 'Boolean':
      renderBoolean(ctrl, moduleName, s);
      break;
    case 'Color':
      renderColor(ctrl, moduleName, s);
      break;
    case 'Int':
    case 'Float':
    case 'Double':
      renderNumber(ctrl, moduleName, s);
      break;
    case 'EnumChoice':
      renderEnumChoice(ctrl, moduleName, s);
      break;
    default:
      renderText(ctrl, moduleName, s);
  }
  return row;
}

function renderBoolean(ctrl, moduleName, s) {
  const cb = document.createElement('input');
  cb.type = 'checkbox';
  cb.checked = !!s.value;
  cb.addEventListener('change', () => saveSetting(moduleName, s.path, cb.checked));
  ctrl.appendChild(cb);
}

function renderNumber(ctrl, moduleName, s) {
  const hasRange = s.min != null && s.max != null;
  if (hasRange) {
    const slider = document.createElement('input');
    slider.type = 'range';
    slider.min = s.min;
    slider.max = s.max;
    slider.step = s.type === 'Int' ? '1' : String((Number(s.max) - Number(s.min)) / 200 || 'any');
    slider.value = s.value;

    const num = document.createElement('input');
    num.type = 'number';
    num.min = s.min;
    num.max = s.max;
    num.step = 'any';
    num.value = s.value;

    slider.addEventListener('input', () => {
      num.value = slider.value;
      debounce(moduleName + s.path, () => saveSetting(moduleName, s.path, Number(slider.value)), 250);
    });
    num.addEventListener('input', () => {
      if (num.value === '') return;
      const v = Number(num.value);
      if (isNaN(v)) return;
      slider.value = v;
      debounce(moduleName + s.path, () => saveSetting(moduleName, s.path, v), 250);
    });

    ctrl.appendChild(slider);
    ctrl.appendChild(num);
    if (s.suffix) {
      const span = document.createElement('span');
      span.className = 'suffix';
      span.textContent = s.suffix;
      ctrl.appendChild(span);
    }
  } else {
    const num = document.createElement('input');
    num.type = 'number';
    num.step = 'any';
    num.value = s.value;
    num.addEventListener('input', () => {
      if (num.value === '') return;
      const v = Number(num.value);
      if (isNaN(v)) return;
      debounce(moduleName + s.path, () => saveSetting(moduleName, s.path, v), 250);
    });
    ctrl.appendChild(num);
    if (s.suffix) {
      const span = document.createElement('span');
      span.className = 'suffix';
      span.textContent = s.suffix;
      ctrl.appendChild(span);
    }
  }
}

function renderColor(ctrl, moduleName, s) {
  const picker = document.createElement('input');
  picker.type = 'color';
  const base = String(s.value || '#000000');
  picker.value = (base.length >= 7 ? '#' + base.slice(1, 7) : '#000000').toUpperCase();

  const text = document.createElement('input');
  text.type = 'text';
  text.value = s.value || '';
  text.spellcheck = false;
  text.placeholder = '#RRGGBB or #RRGGBBAA';

  picker.addEventListener('input', () => {
    text.value = picker.value.toUpperCase();
    debounce(moduleName + s.path, () => saveSetting(moduleName, s.path, picker.value.toUpperCase()), 200);
  });
  text.addEventListener('input', () => {
    const v = text.value.trim();
    const m = v.match(/^#([0-9a-fA-F]{6})([0-9a-fA-F]{2})?$/);
    if (!m) return;
    if (m[1]) picker.value = '#' + m[1].toUpperCase();
    debounce(moduleName + s.path, () => saveSetting(moduleName, s.path, v.toUpperCase()), 250);
  });

  ctrl.appendChild(picker);
  ctrl.appendChild(text);
}

function renderEnumChoice(ctrl, moduleName, s) {
  const sel = document.createElement('select');
  const choices = Array.isArray(s.choices) ? s.choices : [];
  if (!choices.length) {
    const opt = document.createElement('option');
    opt.value = '';
    opt.textContent = s.value == null ? '' : String(s.value);
    sel.appendChild(opt);
  } else {
    for (const c of choices) {
      const opt = document.createElement('option');
      opt.value = c;
      opt.textContent = c;
      sel.appendChild(opt);
    }
    sel.value = s.value == null ? '' : String(s.value);
  }
  sel.addEventListener('change', () => saveSetting(moduleName, s.path, sel.value));
  ctrl.appendChild(sel);
}

function renderText(ctrl, moduleName, s) {
  const inp = document.createElement('input');
  inp.type = 'text';
  inp.value = s.value == null ? '' : String(s.value);
  inp.addEventListener('change', () => saveSetting(moduleName, s.path, inp.value));
  ctrl.appendChild(inp);
}

async function saveSetting(moduleName, path, value) {
  try {
    await api('/api/modules/' + encodeURIComponent(moduleName) + '/settings', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ path, value })
    });
  } catch (e) {
    toast(e.message, true);
  }
}

async function exportConfig() {
  window.location.href = '/api/config/export';
}

importFile.addEventListener('change', async () => {
  const file = importFile.files && importFile.files[0];
  importFile.value = '';
  if (!file) return;
  try {
    const text = await file.text();
    const r = await api('/api/config/import', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: text
    });
    toast(r.message || 'Config imported.');
    loadModules();
    loadSavedFiles();
  } catch (e) {
    toast(e.message, true);
  }
});

async function saveConfig() {
  const name = prompt('Save current config as:', 'my-config');
  if (!name || !name.trim()) return;
  try {
    const r = await api('/api/config/save', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ name: name.trim() })
    });
    toast(r.message || 'Config saved.');
    loadSavedFiles();
  } catch (e) {
    toast(e.message, true);
  }
}

async function loadSavedFiles() {
  try {
    const files = await api('/api/config/files');
    savedSelect.innerHTML = '';
    if (!files.length) {
      const opt = document.createElement('option');
      opt.value = '';
      opt.textContent = '(none saved)';
      savedSelect.appendChild(opt);
      return;
    }
    for (const f of files) {
      const opt = document.createElement('option');
      opt.value = f;
      opt.textContent = f;
      savedSelect.appendChild(opt);
    }
  } catch (e) {
    savedSelect.innerHTML = '<option value="""">(failed to list)</option>';
  }
}

async function loadConfig() {
  const name = savedSelect.value;
  if (!name) {
    toast('Select a saved config first', true);
    return;
  }
  try {
    const r = await api('/api/config/load', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ name })
    });
    toast(r.message || 'Config loaded.');
    loadModules();
  } catch (e) {
    toast(e.message, true);
  }
}

function esc(s) {
  return String(s).replace(/[&<>""]/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '""': '&quot;' }[c]));
}

loadModules();
loadSavedFiles();
</script>
</body>
</html>";
    }
}
