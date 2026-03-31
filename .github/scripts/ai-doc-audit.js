// ai-doc-audit.js — called by actions/github-script@v7 via `script: require(...)`
// Environment variables expected:
//   MODELS_TOKEN      — GitHub PAT with models:read
//   MODELS_API_BASE   — https://models.inference.ai.azure.com
//   MODEL_ID          — e.g. gpt-4o
//   GITHUB_WORKSPACE  — set automatically by Actions

const fs   = require('fs');
const path = require('path');

module.exports = async ({ core }) => {
  const WORKSPACE    = process.env.GITHUB_WORKSPACE || process.cwd();
  const DOCS_ABS_DIR = path.resolve(WORKSPACE, 'docs');

  const sourceCode = fs.existsSync('/tmp/source_code.txt')
    ? fs.readFileSync('/tmp/source_code.txt', 'utf8') : '';
  const docsContent = fs.existsSync('/tmp/docs_content.txt')
    ? fs.readFileSync('/tmp/docs_content.txt', 'utf8') : '';
  const missingDocs = fs.existsSync('/tmp/missing_docs.txt')
    ? fs.readFileSync('/tmp/missing_docs.txt', 'utf8').trim() : '';

  const systemPrompt = [
    'Ти — асистент з документації для Unity C# проекту "moyva".',
    'Документація зберігається у docs/systems/*.md і docs/README.md — написана українською мовою у форматі Markdown.',
    'Джерело правди — код у папці Assets/Moyva/Scripts/ (Bootstrap/ і Features/).',
    '',
    'Твоє завдання — ПОВНИЙ АУДИТ документації:',
    '1. Проаналізуй ВЕСЬ наданий вихідний код.',
    '2. Для кожної системи/фічі перевір чи існує документація і чи вона актуальна.',
    '3. Якщо системи немає в документації — створи новий .md файл.',
    '4. Якщо документація застаріла або неповна — онови її.',
    '5. Системи без документації (вже виявлені): ' + (missingDocs || 'немає (але перевір самостійно)'),
    '',
    'ВІДПОВІДАЙ ВИКЛЮЧНО валідним JSON такого формату:',
    '{',
    '  "needs_update": true | false,',
    '  "reason": "стисле пояснення",',
    '  "files": {',
    '    "docs/systems/system-name.md": "повний Markdown вміст"',
    '  }',
    '}',
    '',
    'Правила:',
    '- Якщо документація повністю актуальна -> needs_update: false, files: {}',
    '- Документуй ТІЛЬКИ системи з Assets/Moyva/Scripts/ (не тести)',
    '- Назва файлу = назва папки системи у нижньому регістрі (напр. objectsmap -> objects-map.md)',
    '- Зберігай українську мову та наявний стиль документації',
    '- Шлях файлу ОБОВ\'ЯЗКОВО має починатися з "docs/"',
  ].join('\n');

  const userMessage =
    '=== ВИХІДНИЙ КОД (Assets/Moyva/Scripts/) ===\n' + sourceCode + '\n\n' +
    '=== ПОТОЧНА ДОКУМЕНТАЦІЯ ===\n' + docsContent;

  console.log('Calling Models API — full audit mode...');

  const apiBase = process.env.MODELS_API_BASE.replace(/\/$/, '');

  // 1. Validate token
  const modelsListRes = await fetch(apiBase + '/models', {
    method: 'GET',
    headers: {
      'Authorization': 'Bearer ' + process.env.MODELS_TOKEN,
      'Accept': 'application/json',
    },
  });

  console.log('Models list status: ' + modelsListRes.status);
  const modelsListText = await modelsListRes.text();
  console.log(modelsListText.substring(0, Math.min(1000, modelsListText.length)));

  if (modelsListRes.status >= 400) {
    throw new Error('Models endpoint returned ' + modelsListRes.status + ': ' + modelsListText);
  }

  // 2. Call chat completions
  const completionRes = await fetch(apiBase + '/chat/completions', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + process.env.MODELS_TOKEN,
      'Accept': 'application/json',
    },
    body: JSON.stringify({
      model: process.env.MODEL_ID,
      messages: [
        { role: 'system', content: systemPrompt },
        { role: 'user',   content: userMessage  },
      ],
      response_format: { type: 'json_object' },
      max_tokens: 2048,
      temperature: 0.1,
    }),
  });

  if (!completionRes.ok) {
    const errBody = await completionRes.text();
    throw new Error('Models API error ' + completionRes.status + ': ' + errBody);
  }

  const data       = await completionRes.json();
  const rawContent = data.choices && data.choices[0] && data.choices[0].message
    ? data.choices[0].message.content : '';
  console.log('AI response length: ' + String(rawContent).length + ' chars');

  let result;
  try {
    result = JSON.parse(rawContent);
  } catch (e) {
    throw new Error('Could not parse AI response as JSON:\n' + String(rawContent).substring(0, 800));
  }

  const needsUpdate = Boolean(result.needs_update);
  const reason      = String(result.reason || '');
  console.log('needs_update : ' + needsUpdate);
  console.log('reason       : ' + reason);

  core.setOutput('needs_update', String(needsUpdate));
  core.setOutput('reason', reason);

  const filesObj =
    needsUpdate &&
    result.files !== null &&
    typeof result.files === 'object' &&
    !Array.isArray(result.files)
      ? result.files
      : {};

  const fileEntries = Object.entries(filesObj);
  if (needsUpdate && fileEntries.length > 0) {
    let filesWritten = 0;

    for (const [filePath, content] of fileEntries) {
      const absTarget = path.resolve(WORKSPACE, filePath);

      if (!absTarget.startsWith(DOCS_ABS_DIR + path.sep) && absTarget !== DOCS_ABS_DIR) {
        console.warn('SKIPPED — path escapes docs/: ' + filePath);
        continue;
      }

      const dir = path.dirname(absTarget);
      fs.mkdirSync(dir, { recursive: true });
      fs.writeFileSync(absTarget, String(content), 'utf8');
      console.log('Written: ' + filePath);
      filesWritten++;
    }

    core.setOutput('docs_updated', filesWritten > 0 ? 'true' : 'false');
  } else {
    core.setOutput('docs_updated', 'false');
  }
};