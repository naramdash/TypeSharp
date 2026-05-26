const fs = require('node:fs');

const index = fs.readFileSync('dist/index.html', 'utf8');
const install = fs.readFileSync('dist/install/index.html', 'utf8');
const startHere = fs.readFileSync('dist/start-here/index.html', 'utf8');
const tutorials = fs.readFileSync('dist/tutorials/index.html', 'utf8');
const learningPaths = fs.readFileSync('dist/learning-paths/index.html', 'utf8');
const languageTour = fs.readFileSync('dist/language-tour/index.html', 'utf8');
const fundamentals = fs.readFileSync('dist/fundamentals/index.html', 'utf8');
const guides = fs.readFileSync('dist/guides/index.html', 'utf8');
const dotnetInterop = fs.readFileSync('dist/dotnet-interop/index.html', 'utf8');
const cookbook = fs.readFileSync('dist/cookbook/index.html', 'utf8');
const api = fs.readFileSync('dist/api/index.html', 'utf8');
const examples = fs.readFileSync('dist/examples/index.html', 'utf8');
const diagnostics = fs.readFileSync('dist/diagnostics/index.html', 'utf8');
const advanced = fs.readFileSync('dist/advanced/index.html', 'utf8');
const modules = fs.readFileSync('dist/modules/index.html', 'utf8');
const typeSystem = fs.readFileSync('dist/type-system/index.html', 'utf8');
const csharpTypeModel = fs.readFileSync('dist/csharp-type-model/index.html', 'utf8');
const csharpMembersOverloads = fs.readFileSync('dist/csharp-members-overloads/index.html', 'utf8');
const featureStatus = fs.readFileSync('dist/feature-status/index.html', 'utf8');
const grammar = fs.readFileSync('dist/grammar/index.html', 'utf8');
const reference = fs.readFileSync('dist/reference/index.html', 'utf8');
const lowering = fs.readFileSync('dist/lowering/index.html', 'utf8');
const cli = fs.readFileSync('dist/cli/index.html', 'utf8');
const projectConfiguration = fs.readFileSync('dist/project-configuration/index.html', 'utf8');
const runtimeArtifacts = fs.readFileSync('dist/runtime-artifacts/index.html', 'utf8');
const vscodeLsp = fs.readFileSync('dist/vscode-lsp/index.html', 'utf8');
const migration = fs.readFileSync('dist/migration/index.html', 'utf8');
const troubleshooting = fs.readFileSync('dist/troubleshooting/index.html', 'utf8');
const goal = fs.readFileSync('dist/goal/index.html', 'utf8');
const requirements = fs.readFileSync('dist/requirements/index.html', 'utf8');
const projectPolicy = fs.readFileSync('dist/project-policy/index.html', 'utf8');
const projectLedger = fs.readFileSync('dist/project-ledger/index.html', 'utf8');
const writingGuide = fs.readFileSync('dist/writing-guide/index.html', 'utf8');
const documentOwnership = fs.readFileSync('dist/document-ownership/index.html', 'utf8');
const sitemapIndex = fs.readFileSync('dist/sitemap-index.xml', 'utf8');
const sitemap = fs.readFileSync('dist/sitemap-0.xml', 'utf8');
const releaseTag = process.env.RELEASE_TAG;

function assertContains(value, expected, message) {
  if (!value.includes(expected)) {
    throw new Error(message);
  }
}

function assertContainsAny(value, expectedValues, message) {
  if (!expectedValues.some((expected) => value.includes(expected))) {
    throw new Error(message);
  }
}

function assertNotContains(value, unexpected, message) {
  if (value.includes(unexpected)) {
    throw new Error(message);
  }
}

function firstExistingIndex(value, expectedValues, message) {
  const indexes = expectedValues
    .map((expected) => value.indexOf(expected))
    .filter((index) => index >= 0);

  if (indexes.length === 0) {
    throw new Error(message);
  }

  return Math.min(...indexes);
}

function assertBefore(value, earlier, later, message) {
  const earlierIndex = value.indexOf(earlier);
  const laterIndex = value.indexOf(later);
  if (earlierIndex < 0 || laterIndex < 0 || earlierIndex >= laterIndex) {
    throw new Error(message);
  }
}

function assertAnyBefore(value, earlierValues, laterValues, message) {
  const earlierIndex = firstExistingIndex(value, earlierValues, message);
  const laterIndex = Array.isArray(laterValues)
    ? firstExistingIndex(value, laterValues, message)
    : value.indexOf(laterValues);
  if (laterIndex < 0 || earlierIndex >= laterIndex) {
    throw new Error(message);
  }
}

function firstMatchIndex(value, pattern, message) {
  const match = pattern.exec(value);
  if (!match || match.index < 0) {
    throw new Error(message);
  }

  return match.index;
}

function assertNoRepoLocalCliCommand(value, label) {
  if (value.includes('dotnet cli\\TypeSharp.Cli')) {
    throw new Error(`Rendered ${label} page must not include repo-local CLI commands.`);
  }
}

function assertDotnetToolInstall(value, label) {
  assertContains(value, 'dotnet tool install --global TypeSharp.Tool', `Rendered ${label} page must document the TypeSharp.Tool global tool install.`);
}

function assertRenderedToolRoute(value, label) {
  assertContains(value, 'TypeSharp.Tool', `Rendered ${label} page must name the CLI tool package.`);
  assertDotnetToolInstall(value, label);
}

function assertRenderedRuntimeRoute(value, label) {
  assertContains(value, 'typesharp runtime-path', `Rendered ${label} page must document the packaged runtime-path command.`);
  assertContains(value, 'TypeSharp.Tool', `Rendered ${label} page must name the installed tool package that carries runtime DLLs.`);
}

function assertRenderedPublicCanonical(value, path, label, allowStaleHostText = false) {
  const canonicalUrl = `https://naramdash.github.io/TypeSharp${path}`;
  assertContains(value, `rel="canonical" href="${canonicalUrl}"`, `Rendered ${label} page must use the public GitHub Pages canonical URL.`);
  assertContains(value, `property="og:url" content="${canonicalUrl}"`, `Rendered ${label} page must use the public GitHub Pages Open Graph URL.`);
  if (!allowStaleHostText) {
    assertNotContains(value, 'https://typesharp.github.io/TypeSharp', `Rendered ${label} page must not use the stale legacy GitHub Pages URL.`);
  }
  assertContains(value, 'href="/TypeSharp/', `Rendered ${label} page must use the GitHub Pages /TypeSharp base path.`);
}

const publicPageRoutes = [
  ['docs home', index, '/'],
  ['Install', install, '/install/'],
  ['Start Here', startHere, '/start-here/'],
  ['Tutorials', tutorials, '/tutorials/'],
  ['Learning Paths', learningPaths, '/learning-paths/'],
  ['Language Tour', languageTour, '/language-tour/'],
  ['Fundamentals', fundamentals, '/fundamentals/'],
  ['Guides', guides, '/guides/'],
  ['.NET Interop', dotnetInterop, '/dotnet-interop/'],
  ['Cookbook', cookbook, '/cookbook/'],
  ['API And CLI Reference', api, '/api/'],
  ['Examples', examples, '/examples/'],
  ['Diagnostics', diagnostics, '/diagnostics/'],
  ['Advanced Topics', advanced, '/advanced/'],
  ['Modules And Imports', modules, '/modules/'],
  ['Type System', typeSystem, '/type-system/'],
  ['C# And CLR Type Model', csharpTypeModel, '/csharp-type-model/'],
  ['C# Members And Overloads', csharpMembersOverloads, '/csharp-members-overloads/'],
  ['Feature Status', featureStatus, '/feature-status/'],
  ['Grammar', grammar, '/grammar/'],
  ['Language Reference', reference, '/reference/'],
  ['Lowering', lowering, '/lowering/'],
  ['CLI', cli, '/cli/'],
  ['Project Configuration', projectConfiguration, '/project-configuration/'],
  ['Runtime Artifacts', runtimeArtifacts, '/runtime-artifacts/'],
  ['VS Code And LSP', vscodeLsp, '/vscode-lsp/'],
  ['Migration', migration, '/migration/'],
  ['Troubleshooting', troubleshooting, '/troubleshooting/'],
  ['Core Goal', goal, '/goal/'],
  ['Project Requirements', requirements, '/requirements/'],
  ['Project Policy', projectPolicy, '/project-policy/', true],
  ['Project Ledger', projectLedger, '/project-ledger/'],
  ['Writing Guide', writingGuide, '/writing-guide/'],
  ['Document Ownership', documentOwnership, '/document-ownership/']
];

const expectedPublicPageRouteCount = 34;
if (publicPageRoutes.length !== expectedPublicPageRouteCount) {
  throw new Error(`Rendered public page route table must contain ${expectedPublicPageRouteCount} routes, found ${publicPageRoutes.length}.`);
}

const exactLegacy404Markers = [
  'Document not found (404)',
  'This page could not be found',
  'Page not found'
];

const publicPageRoutePaths = publicPageRoutes.map(([, , path]) => path);
const duplicatePublicPageRoutePaths = publicPageRoutePaths.filter((path, index) => publicPageRoutePaths.indexOf(path) !== index);
if (duplicatePublicPageRoutePaths.length > 0) {
  throw new Error(`Rendered public page route table contains duplicate paths: ${duplicatePublicPageRoutePaths.join(', ')}.`);
}

function selectPublicPageRoutesByLabel(routes, labels, groupName) {
  const uniqueLabels = [...new Set(labels)];
  if (uniqueLabels.length !== labels.length) {
    throw new Error(`Rendered ${groupName} contains duplicate route labels.`);
  }

  const selectedRoutes = [];
  for (const label of labels) {
    const matchingRoutes = routes.filter(([routeLabel]) => routeLabel === label);
    if (matchingRoutes.length !== 1) {
      throw new Error(`Rendered ${groupName} must select exactly one public page route for ${label}, found ${matchingRoutes.length}.`);
    }

    selectedRoutes.push(matchingRoutes[0]);
  }

  const selectedPaths = selectedRoutes.map(([, , path]) => path);
  const uniqueSelectedPaths = [...new Set(selectedPaths)];
  if (uniqueSelectedPaths.length !== selectedPaths.length) {
    throw new Error(`Rendered ${groupName} contains duplicate selected route paths.`);
  }

  return selectedRoutes;
}

const broaderReleaseRoutes = selectPublicPageRoutesByLabel(publicPageRoutes, [
  'Learning Paths',
  'Language Tour',
  'Fundamentals',
  'Guides',
  'Cookbook',
  'API And CLI Reference',
  'Examples',
  'Diagnostics',
  'Advanced Topics'
], 'broader release routes');

const supportReleaseRoutes = selectPublicPageRoutesByLabel(publicPageRoutes, [
  'CLI',
  'Project Configuration',
  'Runtime Artifacts',
  'VS Code And LSP',
  'Migration',
  'Troubleshooting'
], 'support release routes');

const tutorialReleaseRoutes = selectPublicPageRoutesByLabel(publicPageRoutes, [
  'Tutorials'
], 'tutorial release routes');

if (broaderReleaseRoutes.length !== 9) {
  throw new Error(`Rendered broader release route subset must contain 9 routes, found ${broaderReleaseRoutes.length}.`);
}

if (supportReleaseRoutes.length !== 6) {
  throw new Error(`Rendered support release route subset must contain 6 routes, found ${supportReleaseRoutes.length}.`);
}

if (tutorialReleaseRoutes.length !== 1) {
  throw new Error(`Rendered tutorial release route subset must contain 1 route, found ${tutorialReleaseRoutes.length}.`);
}

const commandPolicyRoutes = [...tutorialReleaseRoutes, ...supportReleaseRoutes, ...broaderReleaseRoutes];
const commandPolicyRoutePaths = commandPolicyRoutes.map(([, , path]) => path);
const uniqueCommandPolicyRoutePaths = [...new Set(commandPolicyRoutePaths)];
if (uniqueCommandPolicyRoutePaths.length !== commandPolicyRoutePaths.length) {
  throw new Error('Rendered command policy route subset contains duplicate public page paths.');
}

for (const [label, page, path, allowStaleHostText] of publicPageRoutes) {
  assertRenderedPublicCanonical(page, path, label, allowStaleHostText);
  for (const exactLegacy404Marker of exactLegacy404Markers) {
    assertNotContains(page, exactLegacy404Marker, `Rendered ${label} page must not include exact legacy 404 marker ${exactLegacy404Marker}.`);
  }
}

assertContains(sitemapIndex, 'https://naramdash.github.io/TypeSharp/sitemap-0.xml', 'Rendered sitemap index must use the public GitHub Pages /TypeSharp URL.');
for (const [label, , path] of publicPageRoutes) {
  assertContains(sitemap, `https://naramdash.github.io/TypeSharp${path}`, `Rendered sitemap must include the public GitHub Pages URL for ${label}.`);
}

assertNotContains(sitemapIndex, 'https://typesharp.github.io/TypeSharp', 'Rendered sitemap index must not use the stale legacy GitHub Pages URL.');
assertNotContains(sitemap, 'https://typesharp.github.io/TypeSharp', 'Rendered sitemap must not use the stale legacy GitHub Pages URL.');

const homeInstallHrefCandidates = ['href="install/"', 'href="/install/"', 'href="/TypeSharp/install/"'];
const homeStartHereHrefCandidates = ['href="start-here/"', 'href="/start-here/"', 'href="/TypeSharp/start-here/"'];
const pageInstallHrefCandidates = ['href="../install/"', 'href="/install/"', 'href="/TypeSharp/install/"'];
const homeInstallSentenceCandidates = [
  'Start with <a href="install/">Install</a>',
  'Start with <a href="/install/">Install</a>',
  'Start with <a href="/TypeSharp/install/">Install</a>'
];

const renderedInstallNavIndex = firstMatchIndex(index, /href="[^"]*\/install\/"/, 'Rendered docs home must include a rendered Install navigation link.');
const renderedStartHereNavIndex = firstMatchIndex(index, /href="[^"]*\/start-here\/"/, 'Rendered docs home must include a rendered Start Here navigation link.');
if (renderedInstallNavIndex >= renderedStartHereNavIndex) {
  throw new Error('Rendered docs navigation must expose Install before Start Here.');
}

const renderedNextIndex = firstMatchIndex(index, /rel="next"/, 'Rendered docs home must include a next-page pagination link.');
const renderedNextWindow = index.slice(Math.max(0, renderedNextIndex - 300), renderedNextIndex + 800);
assertContains(renderedNextWindow, '/install/', 'Rendered docs home next-page link must point to Install.');
assertContains(renderedNextWindow, 'Install', 'Rendered docs home next-page link must label Install.');

assertContainsAny(index, homeInstallHrefCandidates, 'Rendered docs home must link to Install.');
assertContainsAny(index, homeStartHereHrefCandidates, 'Rendered docs home must link to Start Here.');
assertAnyBefore(index, homeInstallHrefCandidates, homeStartHereHrefCandidates, 'Rendered docs home must expose Install before Start Here.');
assertAnyBefore(index, homeInstallSentenceCandidates, '<h2 id="contributor-source-built-development-path">', 'Rendered docs home must keep the release install route before the contributor development path.');
assertRenderedToolRoute(index, 'docs home');
assertContains(index, 'typesharp runtime-path', 'Rendered docs home must name the runtime-path command.');
assertRenderedToolRoute(install, 'Install');
assertRenderedRuntimeRoute(install, 'Install');
if (releaseTag) {
  if (!/^v\d+\.\d+\.\d+(-preview\.\d+)?$/.test(releaseTag)) {
    throw new Error(`RELEASE_TAG must match vMAJOR.MINOR.PATCH or vMAJOR.MINOR.PATCH-preview.N: ${releaseTag}`);
  }

  assertContains(
    install,
    `Build metadata ${releaseTag}`,
    'Rendered Install page version sample must show RELEASE_TAG as build metadata.'
  );
}
assertContainsAny(startHere, pageInstallHrefCandidates, 'Rendered Start Here page must link to Install.');
assertRenderedToolRoute(startHere, 'Start Here');
assertContains(startHere, 'typesharp runtime-path', 'Rendered Start Here page must name the runtime-path command.');
assertBefore(startHere, '<h2 id="install-first">', '<h2 id="contributor-source-built-development-path">', 'Rendered Start Here page must keep Install First before the contributor development path.');
if (startHere.includes('dotnet cli\\TypeSharp.Cli')) {
  assertAnyBefore(startHere, pageInstallHrefCandidates, 'dotnet cli\\TypeSharp.Cli\\bin\\Debug\\net10.0\\typesharp.dll', 'Rendered Start Here page must keep Install before repo-local fallback commands.');
}
assertContainsAny(tutorials, pageInstallHrefCandidates, 'Rendered Tutorials page must link to Install.');
assertRenderedToolRoute(tutorials, 'Tutorials');
assertContains(tutorials, 'typesharp runtime-path', 'Rendered Tutorials page must name the runtime-path command.');
assertBefore(tutorials, 'TypeSharp.Tool', 'typesharp new console', 'Rendered Tutorials page must keep the tool install route before first project commands.');
for (const [label, page] of broaderReleaseRoutes) {
  assertContainsAny(page, pageInstallHrefCandidates, `Rendered ${label} page must link to Install.`);
  assertRenderedRuntimeRoute(page, label);
  assertContains(page, 'TypeSharp.Tool', `Rendered ${label} page must name the CLI tool package.`);
}
assertRenderedRuntimeRoute(cli, 'CLI');
assertContains(cli, 'TypeSharp.Tool', 'Rendered CLI page must name the CLI tool package.');
assertContainsAny(cli, pageInstallHrefCandidates, 'Rendered CLI page must link to Install.');
assertRenderedRuntimeRoute(projectConfiguration, 'Project Configuration');
assertContainsAny(projectConfiguration, pageInstallHrefCandidates, 'Rendered Project Configuration page must link to Install.');
assertContains(projectConfiguration, 'TypeSharp.Tool-runtime-root/TypeSharp.Core.dll', 'Rendered Project Configuration page must show runtime Core DLL paths from the installed tool package.');
assertContains(projectConfiguration, 'TypeSharp.Tool-runtime-root/TypeSharp.Runtime.dll', 'Rendered Project Configuration page must show runtime helper DLL paths from the installed tool package.');
assertRenderedRuntimeRoute(runtimeArtifacts, 'Runtime Artifacts');
assertContainsAny(runtimeArtifacts, pageInstallHrefCandidates, 'Rendered Runtime Artifacts page must link to Install.');
assertContains(runtimeArtifacts, 'TypeSharp.Tool', 'Rendered Runtime Artifacts page must name the CLI tool package.');
assertContains(runtimeArtifacts, 'generated/bin/', 'Rendered Runtime Artifacts page must show generated net48 output paths.');
assertContains(runtimeArtifacts, 'TypeSharp.Tool-runtime-root/TypeSharp.Core.dll', 'Rendered Runtime Artifacts page must show runtime Core DLL paths from the installed tool package.');
assertContains(runtimeArtifacts, 'TypeSharp.Tool-runtime-root/TypeSharp.Runtime.dll', 'Rendered Runtime Artifacts page must show runtime helper DLL paths from the installed tool package.');
assertContainsAny(vscodeLsp, pageInstallHrefCandidates, 'Rendered VS Code And LSP page must link to Install.');
assertContains(vscodeLsp, 'code --install-extension', 'Rendered VS Code And LSP page must document VSIX installation.');
assertRenderedRuntimeRoute(migration, 'Migration');
assertContainsAny(migration, pageInstallHrefCandidates, 'Rendered Migration page must link to Install.');
assertRenderedRuntimeRoute(troubleshooting, 'Troubleshooting');
assertContainsAny(troubleshooting, pageInstallHrefCandidates, 'Rendered Troubleshooting page must link to Install.');
assertContains(troubleshooting, 'generated/bin/', 'Rendered Troubleshooting page must show generated net48 output paths.');
assertContains(troubleshooting, 'TypeSharp.Tool-runtime-root/TypeSharp.Core.dll', 'Rendered Troubleshooting page must show runtime Core DLL paths from the installed tool package.');
assertContains(troubleshooting, 'TypeSharp.Tool-runtime-root/TypeSharp.Runtime.dll', 'Rendered Troubleshooting page must show runtime helper DLL paths from the installed tool package.');
for (const [label, page] of commandPolicyRoutes) {
  assertNoRepoLocalCliCommand(page, label);
}
