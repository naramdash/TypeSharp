import { defineConfig } from 'astro/config';
import starlight from '@astrojs/starlight';

const repository = process.env.GITHUB_REPOSITORY ?? '';
const repositoryName = repository.includes('/') ? repository.split('/')[1] : '';
const base = process.env.TYPE_SHARP_DOCS_BASE ?? (repositoryName ? `/${repositoryName}` : '/');
const site = process.env.TYPE_SHARP_DOCS_SITE ?? 'https://typesharp.github.io';

export default defineConfig({
  site,
  base,
  integrations: [
    starlight({
      title: 'TypeSharp',
      description: '.NET Framework 4.8-compatible static language design, compiler, CLI, and VS Code tooling.',
      sidebar: [
        {
          label: 'Learn',
          items: [
            { label: 'Overview', slug: 'index' },
            { label: 'Start Here', slug: 'start-here' },
            { label: 'Tutorials', slug: 'tutorials' },
            { label: 'Fundamentals', slug: 'fundamentals' },
          ],
        },
        {
          label: 'Use TypeSharp',
          items: [
            { label: 'Guides', slug: 'guides' },
            { label: 'Cookbook', slug: 'cookbook' },
            { label: 'Examples', slug: 'examples' },
            { label: 'Migration', slug: 'migration' },
          ],
        },
        {
          label: 'Reference',
          items: [
            { label: 'Grammar', slug: 'grammar' },
            { label: 'Language Reference', slug: 'reference' },
            { label: 'API And CLI Reference', slug: 'api' },
            { label: 'CLI', slug: 'cli' },
            { label: 'Diagnostics', slug: 'diagnostics' },
          ],
        },
        {
          label: 'Tools And Project',
          items: [
            { label: 'VS Code And LSP', slug: 'vscode-lsp' },
            { label: 'Troubleshooting', slug: 'troubleshooting' },
            { label: 'Core Goal', slug: 'goal' },
          ],
        },
      ],
    }),
  ],
});
