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
          label: 'Start',
          items: [
            { label: 'Overview', slug: 'index' },
            { label: 'Core Goal', slug: 'goal' },
          ],
        },
        {
          label: 'Language',
          items: [
            { label: 'Grammar', slug: 'grammar' },
            { label: 'Migration', slug: 'migration' },
          ],
        },
        {
          label: 'Tooling',
          items: [
            { label: 'CLI', slug: 'cli' },
            { label: 'Diagnostics', slug: 'diagnostics' },
            { label: 'VS Code And LSP', slug: 'vscode-lsp' },
          ],
        },
        {
          label: 'Adoption',
          items: [
            { label: 'Examples', slug: 'examples' },
          ],
        },
      ],
    }),
  ],
});
