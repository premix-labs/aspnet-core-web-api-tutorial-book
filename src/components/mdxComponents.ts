// mdxComponents.ts — Components made available to every chapter's MDX by name,
// so authors write <Callout>, <Figure>, <Steps>, <Tabs>/<Tab>, <Kbd>, <Badge>
// without importing anything per file. Wired up in src/pages/[slug].astro.
import Callout from './Callout.astro';
import Figure from './Figure.astro';
import Steps from './Steps.astro';
import Tabs from './Tabs.astro';
import Tab from './Tab.astro';
import Kbd from './Kbd.astro';
import Badge from './Badge.astro';

export const mdxComponents = {
  Callout,
  Figure,
  Steps,
  Tabs,
  Tab,
  Kbd,
  Badge,
};
