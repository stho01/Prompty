import { defineConfig } from 'vitepress'

export default defineConfig({
  title: "Promty",
  description: "A powerful command-line parser and command executor framework for .NET",
  base: '/promty/',

  themeConfig: {
    logo: '/logo.png',

    nav: [
      { text: 'Home', link: '/' },
      { text: 'Guide', link: '/guide/getting-started' },
      { text: 'API Reference', link: '/api/commands' },
      { text: 'Examples', link: '/examples/basic' }
    ],

    sidebar: [
      {
        text: 'Guide',
        items: [
          { text: 'Getting Started', link: '/guide/getting-started' },
          { text: 'Commands', link: '/guide/commands' },
          { text: 'Arguments & Flags', link: '/guide/arguments' },
          { text: 'Flags Enums', link: '/guide/flags-enums' },
          { text: 'Process Commands', link: '/guide/process-commands' },
          { text: 'Help Text', link: '/guide/help-text' }
        ]
      },
      {
        text: 'API Reference',
        items: [
          { text: 'Commands', link: '/api/commands' },
          { text: 'Attributes', link: '/api/attributes' },
          { text: 'Command Executor', link: '/api/executor' }
        ]
      },
      {
        text: 'Examples',
        items: [
          { text: 'Basic Command', link: '/examples/basic' },
          { text: 'File Operations', link: '/examples/file-operations' },
          { text: 'Flags Enums', link: '/examples/flags-enums' },
          { text: 'Process Wrapper', link: '/examples/process-wrapper' }
        ]
      }
    ],

    socialLinks: [
      { icon: 'github', link: 'https://github.com/stho01/promty' },
      { icon: 'npm', link: 'https://www.nuget.org/packages/Promty' }
    ],

    footer: {
      message: 'Released under the MIT License.',
      copyright: 'Copyright © 2024-present STHO'
    },

    search: {
      provider: 'local'
    }
  }
})
