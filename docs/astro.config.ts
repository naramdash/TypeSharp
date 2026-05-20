import { createRequire } from 'node:module';
import { defineConfig } from 'astro/config';
import mermaid from 'astro-mermaid';
import starlight from '@astrojs/starlight';

const require = createRequire(import.meta.url);
const repository = process.env.GITHUB_REPOSITORY ?? '';
const repositoryName = repository.includes('/') ? repository.split('/')[1] : '';
const base = process.env.TYPE_SHARP_DOCS_BASE ?? (repositoryName ? `/${repositoryName}` : '/');
const site = process.env.TYPE_SHARP_DOCS_SITE ?? 'https://typesharp.github.io';
const typesharpTextMateGrammar = require('../vscode/typesharp/syntaxes/typesharp.tmLanguage.json') as {
  name: string;
  scopeName: string;
  fileTypes: string[];
  patterns: unknown[];
  repository: Record<string, unknown>;
};
const typesharpShikiLanguage = {
  ...typesharpTextMateGrammar,
  name: 'typesharp',
  displayName: 'TypeSharp',
  aliases: ['tysh'],
};

export default defineConfig({
  site,
  base,
  integrations: [
    mermaid({
      autoTheme: true,
      enableLog: false,
    }),
    starlight({
      title: 'TypeSharp',
      description: '.NET Framework 4.8-compatible static language design, compiler, CLI, and VS Code tooling.',
      disable404Route: true,
      expressiveCode: {
        shiki: {
          langs: [typesharpShikiLanguage],
          langAlias: {
            typesharp: 'typesharp',
            tysh: 'typesharp',
          },
        },
      },
      sidebar: [
        {
          label: 'Learn',
          items: [
            { label: 'Overview', slug: 'index' },
            { label: 'Start Here', slug: 'start-here' },
            { label: 'Learning Paths', slug: 'learning-paths' },
            { label: 'Language Tour', slug: 'language-tour' },
            { label: 'Tutorials', slug: 'tutorials' },
            { label: 'Fundamentals', slug: 'fundamentals' },
          ],
        },
        {
          label: 'Use TypeSharp',
          items: [
            { label: 'Guides', slug: 'guides' },
            { label: 'Project Configuration', slug: 'project-configuration' },
            { label: 'Runtime Artifacts', slug: 'runtime-artifacts' },
            { label: '.NET Interop', slug: 'dotnet-interop' },
            { label: 'Cookbook', slug: 'cookbook' },
            { label: 'Examples', slug: 'examples' },
            { label: 'Migration', slug: 'migration' },
          ],
        },
        {
          label: 'Reference',
          items: [
            { label: 'Modules And Imports', slug: 'modules' },
            { label: 'Type System', slug: 'type-system' },
            { label: 'Feature Status', slug: 'feature-status' },
            { label: 'Grammar', slug: 'grammar' },
            { label: 'Language Reference', slug: 'reference' },
            { label: 'Lowering', slug: 'lowering' },
            { label: 'API And CLI Reference', slug: 'api' },
            { label: 'CLI', slug: 'cli' },
            { label: 'Diagnostics', slug: 'diagnostics' },
            { label: 'Advanced Topics', slug: 'advanced' },
          ],
        },
        {
          label: 'Tools And Project',
          items: [
            { label: 'VS Code And LSP', slug: 'vscode-lsp' },
            { label: 'Troubleshooting', slug: 'troubleshooting' },
            { label: 'Core Goal', slug: 'goal' },
            { label: 'Project Requirements', slug: 'requirements' },
            { label: 'Project Policy', slug: 'project-policy' },
            { label: 'Project Ledger', slug: 'project-ledger' },
            { label: 'Work Ledger', slug: 'work-ledger' },
            { label: 'Document Ownership', slug: 'document-ownership' },
            { label: 'Agentic Workflow', slug: 'agentic-workflow' },
          ],
        },
      ],
    } satisfies Parameters<typeof starlight>[0]),
  ],
});
