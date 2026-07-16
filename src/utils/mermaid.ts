// mermaid.ts — Client-side Mermaid diagram rendering.
//
// Authors just write a normal fenced code block:
//
//   ```mermaid
//   graph TD
//     A --> B
//   ```
//
// Shiki syntax-highlights it like any other code block at build time (progressive
// enhancement baseline — still readable with JS disabled). On the client, this module
// finds those blocks after render and replaces them with a rendered SVG diagram. The
// `mermaid` package itself (~sizeable) is only ever dynamically imported when a page
// actually has a diagram on it.

let mermaidModulePromise: Promise<typeof import('mermaid').default> | null = null;
let diagramCounter = 0;

function loadMermaid() {
  if (!mermaidModulePromise) {
    mermaidModulePromise = import('mermaid').then((m) => m.default);
  }
  return mermaidModulePromise;
}

function currentTheme(): 'dark' | 'default' {
  return document.documentElement.getAttribute('data-theme') === 'light' ? 'default' : 'dark';
}

async function renderInto(mermaid: Awaited<ReturnType<typeof loadMermaid>>, wrapper: HTMLElement, source: string) {
  const id = `mermaid-diagram-${diagramCounter++}`;
  try {
    const { svg } = await mermaid.render(id, source);
    wrapper.innerHTML = svg;
    wrapper.removeAttribute('aria-busy');
  } catch (err) {
    wrapper.innerHTML = '<p class="mermaid-error">Diagram failed to render.</p>';
    wrapper.removeAttribute('aria-busy');
    console.warn('[mermaid] render failed:', err);
  }
}

/**
 * Finds ```mermaid blocks under `root` and replaces each with a rendered diagram.
 * Shiki marks the language via `pre[data-language]`, never as a class on `<code>`.
 */
export async function renderMermaidDiagrams(root: ParentNode = document): Promise<void> {
  const blocks = Array.from(root.querySelectorAll<HTMLElement>('pre[data-language="mermaid"]'));
  if (blocks.length === 0) return;

  const mermaid = await loadMermaid();
  mermaid.initialize({ startOnLoad: false, theme: currentTheme(), securityLevel: 'strict', fontFamily: 'inherit' });

  for (const pre of blocks) {
    const code = pre.querySelector('code');
    const source = code?.textContent ?? '';

    const wrapper = document.createElement('div');
    wrapper.className = 'mermaid-diagram';
    wrapper.setAttribute('role', 'img');
    wrapper.setAttribute('aria-busy', 'true');
    wrapper.dataset.mermaidSource = source;
    pre.replaceWith(wrapper);

    await renderInto(mermaid, wrapper, source);
  }
}

/** Re-renders every already-rendered diagram on the page — call after a theme switch. */
export async function rerenderMermaidTheme(): Promise<void> {
  const wrappers = Array.from(document.querySelectorAll<HTMLElement>('.mermaid-diagram[data-mermaid-source]'));
  if (wrappers.length === 0) return;

  const mermaid = await loadMermaid();
  mermaid.initialize({ startOnLoad: false, theme: currentTheme(), securityLevel: 'strict', fontFamily: 'inherit' });

  for (const wrapper of wrappers) {
    await renderInto(mermaid, wrapper, wrapper.dataset.mermaidSource ?? '');
  }
}
