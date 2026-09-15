import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { StdioServerTransport } from "@modelcontextprotocol/sdk/server/stdio.js";
import { z } from "zod";

import fs from "node:fs/promises";
import path from "node:path";
import crypto from "node:crypto";


const ROOT = path.resolve(
  process.env.MOYVA_ROOT || process.cwd()
);

const OLLAMA =
  process.env.OLLAMA_BASE_URL ||
  "http://127.0.0.1:11434";

const CACHE_DIR = path.join(
  ROOT,
  ".ai",
  "council",
  "cache"
);


const MODELS = {
  scout:
    process.env.COUNCIL_QWEN7B ||
    "qwen2.5-coder:7b",

  reasoner:
    process.env.COUNCIL_GPT_OSS ||
    "gpt-oss:20b",

  engineer:
    process.env.COUNCIL_DEVSTRAL ||
    "devstral:24b",

  architect:
    process.env.COUNCIL_QWEN30B ||
    "qwen3-coder:30b"
};


const MODEL_CONFIG = {

  scout: {
    num_ctx: 16384,
    num_predict: 2200,
    timeout: 20 * 60 * 1000
  },

  reasoner: {
    num_ctx: 12288,
    num_predict: 2600,
    timeout: 35 * 60 * 1000
  },

  engineer: {
    num_ctx: 8192,
    num_predict: 2400,
    timeout: 40 * 60 * 1000
  },

  architect: {
    num_ctx: 8192,
    num_predict: 2400,
    timeout: 50 * 60 * 1000
  }
};


const SYSTEMS = {

  scout: `
You are the Scout and code investigator for the Moyva Unity project.

You do NOT independently browse the repository.
Use only the supplied source files and evidence.

Primary production source:
Assets/Moyva/Scripts/

Your responsibilities:

- identify concrete classes, methods and symbols;
- map visible dependencies;
- identify call relationships;
- identify responsibilities;
- identify duplication;
- identify suspicious coupling;
- find obvious maintainability problems;
- identify missing evidence;
- propose small and safe improvements.

Never invent:

- classes
- methods
- APIs
- usages
- constructors
- dependencies
- runtime behavior
- Unity APIs
- Zenject APIs

Distinguish verified evidence from assumptions.

Be concise and evidence-based.
`,


  reasoner: `
You are the skeptical architecture reviewer for the Moyva Unity project.

Use ONLY supplied current source code and verified evidence.

Your primary responsibility is to challenge conclusions.

Actively try to DISPROVE proposed findings.

For important claims use:

CONFIRMED
PARTIALLY_CONFIRMED
UNVERIFIED
REJECTED

Check whether:

- the behavior may be intentional;
- responsibilities are genuinely independent;
- a proposed abstraction solves a real problem;
- an abstraction is speculative;
- Unity serialization may create hidden references;
- prefabs or scenes may depend on the code;
- DI / Zenject ownership changes;
- multiplayer behavior may change;
- save/persistence behavior may change;
- contrary evidence exists;
- another implementation already solves the issue.

Never invent code or repository facts.

Prefer minimal architecture over abstraction for abstraction's sake.
`,


  engineer: `
You are the senior software engineer responsible for practical and safe
refactoring of the Moyva Unity C# project.

Use only supplied current source evidence.

Focus on:

- practical multi-file refactoring;
- cohesive responsibilities;
- minimal changes;
- preserving behavior;
- preserving public APIs when reasonable;
- Unity serialization safety;
- Zenject composition roots;
- integration impact;
- migration requirements;
- verification strategy;
- reducing unnecessary coupling;
- avoiding duplication.

Do NOT split a class merely because it is large.

Do NOT create unnecessary:

- interfaces
- managers
- helper classes
- wrappers
- abstractions

Prefer the smallest cohesive refactor that solves a VERIFIED problem.
`,


  architect: `
You are the senior architecture judge for Moyva.

This role is reserved for difficult L3 architectural decisions.

Use only supplied current source evidence.

Evaluate:

- system boundaries;
- ownership;
- lifecycle;
- dependency direction;
- DI/composition roots;
- state ownership;
- multiplayer authority;
- persistence boundaries;
- save/load implications;
- long-term maintainability;
- second-order effects;
- migration risk.

Challenge previous conclusions.

Avoid speculative repository-wide rewrites.

For important statements distinguish:

VERIFIED FACT
INFERENCE
UNKNOWN

If evidence is insufficient, explicitly state which source files,
symbols, tests or runtime evidence are still required.

Prefer the smallest architecture change that resolves the actual problem.
`,


  capsule: `
Create an extremely compact handoff for an expensive external coding model.

The external model must NOT repeat repository discovery.

Output exactly these sections:

GOAL

CURRENT VERIFIED BEHAVIOR

VERIFIED PROBLEM

RELEVANT FILES

IMPORTANT SYMBOLS

CONSTRAINTS

LOCAL ANALYSIS

PROPOSED PLAN

OPEN QUESTIONS

EXACT TASK FOR PAID MODEL

Remove redundant explanation.

Do not include large raw source blocks unless absolutely necessary.

Preserve exact file paths and important symbol names.

The objective is to minimize paid API input tokens while preserving all
information necessary for high-quality reasoning.
`
};


await fs.mkdir(
  CACHE_DIR,
  { recursive: true }
);


function safeText(value, max = 8000) {

  if (!value) {
    return "";
  }

  const str = String(value);

  if (str.length <= max) {
    return str;
  }

  const half =
    Math.floor(max / 2);

  return (
    str.slice(0, half) +
    "\n\n...[TRUNCATED]...\n\n" +
    str.slice(-half)
  );
}


async function loadFiles(files = []) {

  const unique =
    [...new Set(files)]
      .slice(0, 20);

  let output = "";
  let used = 0;

  const TOTAL_LIMIT = 24000;
  const PER_FILE_LIMIT = 9000;

  for (const requested of unique) {

    if (used >= TOTAL_LIMIT) {
      break;
    }

    try {

      const full =
        path.resolve(ROOT, requested);

      const rootPrefix =
        ROOT.endsWith(path.sep)
          ? ROOT
          : ROOT + path.sep;


      if (
        full !== ROOT &&
        !full.startsWith(rootPrefix)
      ) {

        output +=
          `\n[REJECTED OUTSIDE WORKSPACE: ${requested}]\n`;

        continue;
      }


      const stat =
        await fs.stat(full);


      if (!stat.isFile()) {

        output +=
          `\n[NOT A FILE: ${requested}]\n`;

        continue;
      }


      let content =
        await fs.readFile(
          full,
          "utf8"
        );


      content =
        safeText(
          content,
          Math.min(
            PER_FILE_LIMIT,
            TOTAL_LIMIT - used
          )
        );


      const block =
        `\n\n===== FILE: ${requested} =====\n` +
        content;


      output += block;
      used += block.length;
    }

    catch (error) {

      output +=
        `\n[FAILED TO READ ${requested}: ${error.message}]\n`;
    }
  }


  return output;
}


function hash(value) {

  return crypto
    .createHash("sha256")
    .update(value)
    .digest("hex");
}


async function ollamaRequest({
  role,
  model,
  system,
  prompt,
  refresh = false
}) {

  const cfg =
    MODEL_CONFIG[role] ||
    MODEL_CONFIG.reasoner;


  const cacheKey =
    hash(
      JSON.stringify({
        model,
        system,
        prompt,
        cfg
      })
    );


  const cacheFile =
    path.join(
      CACHE_DIR,
      cacheKey + ".json"
    );


  if (!refresh) {

    try {

      const cached =
        JSON.parse(
          await fs.readFile(
            cacheFile,
            "utf8"
          )
        );


      return {
        ...cached,
        cached: true
      };
    }

    catch {
      // No cache
    }
  }


  const controller =
    new AbortController();


  const timer =
    setTimeout(
      () => controller.abort(),
      cfg.timeout
    );


  const started =
    Date.now();


  try {

    console.error(
      `[Moyva Council] Loading ${model}`
    );


    const response =
      await fetch(
        `${OLLAMA}/api/chat`,
        {

          method: "POST",

          headers: {
            "Content-Type": "application/json"
          },

          signal: controller.signal,

          body: JSON.stringify({

            model,

            messages: [

              {
                role: "system",
                content: system
              },

              {
                role: "user",
                content: prompt
              }

            ],

            stream: false,

            /*
             * Unload model immediately after the request.
             * Important on a 16 GB laptop.
             */
            keep_alive: 0,

            options: {

              num_ctx:
                cfg.num_ctx,

              num_predict:
                cfg.num_predict,

              temperature:
                0.12,

              top_p:
                0.9
            }
          })
        }
      );


    if (!response.ok) {

      const errorText =
        await response.text();


      throw new Error(
        `Ollama HTTP ${response.status}: ${errorText}`
      );
    }


    const data =
      await response.json();


    const result = {

      model,

      text:
        data?.message?.content?.trim() ||
        "[Model returned no normal response content]",

      durationSeconds:
        Math.round(
          (Date.now() - started) / 1000
        ),

      promptTokens:
        data?.prompt_eval_count ??
        null,

      outputTokens:
        data?.eval_count ??
        null,

      cached:
        false
    };


    await fs.writeFile(
      cacheFile,
      JSON.stringify(
        result,
        null,
        2
      ),
      "utf8"
    );


    console.error(
      `[Moyva Council] Finished ${model} in ${result.durationSeconds}s`
    );


    return result;
  }

  finally {

    clearTimeout(timer);
  }
}


async function makePrompt({
  task,
  files,
  evidence
}) {

  const fileContext =
    await loadFiles(
      files || []
    );


  const evidenceText =
    safeText(
      evidence || "",
      7000
    );


  return `
TASK

${task}

SUPPLIED VERIFIED EVIDENCE

${evidenceText || "[none]"}

SOURCE FILE CONTEXT

${fileContext || "[no source files supplied]"}

IMPORTANT RULES

Do not assume access to repository files that were not supplied.

Do not invent missing implementation details.

If additional evidence is required, identify the exact files,
classes, methods or symbols that must be inspected.
`;
}


function toolResult(text) {

  return {

    content: [

      {
        type: "text",
        text
      }

    ]
  };
}


function toolError(text) {

  return {

    isError: true,

    content: [

      {
        type: "text",
        text
      }

    ]
  };
}


function formatResult(
  title,
  result
) {

  return `
## ${title}

Model: ${result.model}
Cached: ${result.cached ? "yes" : "no"}
Duration: ${result.durationSeconds}s
Prompt tokens: ${result.promptTokens ?? "unknown"}
Output tokens: ${result.outputTokens ?? "unknown"}

${result.text}
`;
}


const CommonInput =
  z.object({

    task:
      z.string()
        .describe(
          "Exact task/question for the specialist model"
        ),

    files:
      z.array(
        z.string()
      )
        .optional()
        .describe(
          "Workspace-relative source files. Prefer only 3-12 directly relevant files."
        ),

    evidence:
      z.string()
        .optional()
        .describe(
          "Compact verified evidence, constraints, findings, or current hypothesis"
        ),

    refresh:
      z.boolean()
        .optional()
        .default(false)
        .describe(
          "Ignore cached result and run the model again"
        )
  });


const server =
  new McpServer(
    {
      name: "Moyva AI Council",
      version: "1.0.0"
    }
  );


server.registerTool(

  "council_status",

  {

    title:
      "Moyva AI Council Status",

    description:
      "Check Ollama and show which Council models are installed or currently loaded.",

    inputSchema:
      z.object({})
  },

  async () => {

    try {

      const [
        tagsResponse,
        psResponse
      ] =
        await Promise.all([

          fetch(
            `${OLLAMA}/api/tags`
          ),

          fetch(
            `${OLLAMA}/api/ps`
          )

        ]);


      if (!tagsResponse.ok) {

        throw new Error(
          `Ollama tags HTTP ${tagsResponse.status}`
        );
      }


      const tags =
        await tagsResponse.json();


      let ps = {
        models: []
      };


      if (psResponse.ok) {

        ps =
          await psResponse.json();
      }


      const installed =
        (tags.models || [])
          .map(
            m =>
              m.name ||
              m.model
          );


      const loaded =
        (ps.models || [])
          .map(
            m =>
              m.name ||
              m.model
          );


      let text =
        `Ollama: ${OLLAMA}\n\n`;


      for (
        const [role, model]
        of Object.entries(MODELS)
      ) {

        text +=
          `${role}: ${model}\n` +

          `  installed: ${
            installed.includes(model)
              ? "YES"
              : "NO"
          }\n` +

          `  loaded: ${
            loaded.includes(model)
              ? "YES"
              : "NO"
          }\n\n`;
      }


      return toolResult(text);
    }

    catch (error) {

      return toolError(
        `Cannot reach Ollama: ${error.message}`
      );
    }
  }
);


function registerConsultTool(
  name,
  title,
  description,
  role
) {

  server.registerTool(

    name,

    {

      title,

      description,

      inputSchema:
        CommonInput
    },

    async ({
      task,
      files,
      evidence,
      refresh
    }) => {

      try {

        const prompt =
          await makePrompt({
            task,
            files,
            evidence
          });


        const result =
          await ollamaRequest({

            role,

            model:
              MODELS[role],

            system:
              SYSTEMS[role],

            prompt,

            refresh
          });


        return toolResult(
          formatResult(
            title,
            result
          )
        );
      }

      catch (error) {

        return toolError(
          `${title} failed:\n${error.message}`
        );
      }
    }
  );
}


registerConsultTool(

  "consult_qwen7b",

  "Qwen 7B Scout",

  "Fast local code scout for source evidence, responsibilities, dependencies, usages and small refactors.",

  "scout"
);


registerConsultTool(

  "consult_gpt_oss",

  "GPT-OSS Reasoner",

  "Skeptical reasoning reviewer used to challenge assumptions, compare alternatives and validate architecture findings.",

  "reasoner"
);


registerConsultTool(

  "consult_devstral",

  "Devstral Engineer",

  "Software engineering specialist for multi-file refactors, integration impact, implementation planning and review.",

  "engineer"
);


registerConsultTool(

  "consult_qwen30b",

  "Qwen3 Coder Architect",

  "Heavy local architecture reviewer. Use only for difficult L3/high-risk decisions or unresolved disagreement.",

  "architect"
);


server.registerTool(

  "review_with_council",

  {

    title:
      "Review with Moyva AI Council",

    description:
      "Consult several local models sequentially. fast = Qwen7B + GPT-OSS; refactor = plus Devstral; full = plus Qwen3-Coder 30B.",

    inputSchema:
      z.object({

        task:
          z.string(),

        files:
          z.array(
            z.string()
          )
            .optional(),

        evidence:
          z.string()
            .optional(),

        refresh:
          z.boolean()
            .optional()
            .default(false),

        mode:
          z.enum([
            "fast",
            "refactor",
            "full"
          ])
            .default("fast")
      })
  },

  async ({
    task,
    files,
    evidence,
    refresh,
    mode
  }) => {

    try {

      const prompt =
        await makePrompt({
          task,
          files,
          evidence
        });


      const roles =

        mode === "full"

          ? [
              "scout",
              "reasoner",
              "engineer",
              "architect"
            ]

          : mode === "refactor"

            ? [
                "scout",
                "reasoner",
                "engineer"
              ]

            : [
                "scout",
                "reasoner"
              ];


      const titles = {

        scout:
          "Qwen 7B Scout",

        reasoner:
          "GPT-OSS Reasoner",

        engineer:
          "Devstral Engineer",

        architect:
          "Qwen3 Coder Architect"
      };


      let output =
        `# Moyva AI Council — ${mode}\n`;


      for (const role of roles) {

        try {

          const result =
            await ollamaRequest({

              role,

              model:
                MODELS[role],

              system:
                SYSTEMS[role],

              prompt,

              refresh
            });


          output +=
            formatResult(
              titles[role],
              result
            );
        }

        catch (error) {

          output +=
            `\n## ${titles[role]}\nFAILED: ${error.message}\n`;
        }
      }


      output += `

## Manager instruction

Compare the independent opinions above against actual repository evidence.

Do not treat majority vote as proof.

Resolve disagreements by reading current source through Continue tools.

Reject unsupported claims.

If evidence is still insufficient, identify the exact missing source evidence.
`;


      return toolResult(output);
    }

    catch (error) {

      return toolError(
        `Council review failed: ${error.message}`
      );
    }
  }
);


server.registerTool(

  "prepare_paid_ai_capsule",

  {

    title:
      "Prepare Paid AI Capsule",

    description:
      "Compress verified local investigation into a minimal handoff for GPT, Codex or another paid API model.",

    inputSchema:
      CommonInput
  },

  async ({
    task,
    files,
    evidence,
    refresh
  }) => {

    try {

      const prompt =
        await makePrompt({
          task,
          files,
          evidence
        });


      const result =
        await ollamaRequest({

          role:
            "reasoner",

          model:
            MODELS.reasoner,

          system:
            SYSTEMS.capsule,

          prompt,

          refresh
        });


      return toolResult(
        formatResult(
          "Paid AI Context Capsule",
          result
        )
      );
    }

    catch (error) {

      return toolError(
        `Capsule generation failed: ${error.message}`
      );
    }
  }
);


const transport =
  new StdioServerTransport();


await server.connect(
  transport
);


console.error(
  "[Moyva Council] MCP server connected"
);