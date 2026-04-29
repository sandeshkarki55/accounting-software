import { createTheme, MantineColorsTuple } from '@mantine/core';

// Professional accounting color palette
const navy: MantineColorsTuple = [
  '#eef3ff', '#d5e0fd', '#b3c9f5', '#8aadef', '#6a95e9', '#5784e5', '#4b7be4', '#3c69cc', '#335db7', '#2650a2',
];

const teal: MantineColorsTuple = [
  '#e6fcf5', '#d4f5e8', '#a8e9d0', '#77ddb7', '#54d3a3', '#40cc96', '#33c990', '#25b17c', '#1b9e6d', '#00885a',
];

const slate: MantineColorsTuple = [
  '#f5f6f8', '#e7e8ea', '#c9ccd2', '#a8adb8', '#8d93a0', '#7b8292', '#737b8b', '#616979', '#565d6c', '#48505f',
];

export const theme = createTheme({
  primaryColor: 'navy',
  colors: {
    navy,
    teal,
    slate,
  },
  
  fontFamily: '-apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif',
  fontFamilyMonospace: '"JetBrains Mono", "Fira Code", "Cascadia Code", monospace',
  
  headings: {
    fontFamily: '-apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif',
    fontWeight: '600',
  },

  defaultRadius: 'md',
  
  primaryShade: { light: 7, dark: 5 },
  
  components: {
    Table: {
      defaultProps: {
        striped: true,
        highlightOnHover: true,
        withTableBorder: true,
        withColumnBorders: false,
      },
    },
    Badge: {
      defaultProps: {
        variant: 'light',
      },
    },
    Paper: {
      defaultProps: {
        shadow: 'sm',
        p: 'md',
        radius: 'md',
        withBorder: true,
      },
    },
    Card: {
      defaultProps: {
        shadow: 'sm',
        padding: 'lg',
        radius: 'md',
        withBorder: true,
      },
    },
    Button: {
      defaultProps: {
        radius: 'md',
      },
    },
    TextInput: {
      defaultProps: {
        radius: 'md',
      },
    },
    Select: {
      defaultProps: {
        radius: 'md',
      },
    },
    Modal: {
      defaultProps: {
        radius: 'md',
        padding: 'lg',
        overlayProps: {
          backgroundOpacity: 0.55,
          blur: 3,
        },
      },
    },
    NavLink: {
      defaultProps: {
        fw: 500,
      },
    },
  },
});
