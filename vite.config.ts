import { defineConfig } from "vite-plus";

export default defineConfig({
  lint: {
    ignorePatterns: ["dist/**", "node_modules/**", "packages/api/**"],
    jsPlugins: ["eslint-plugin-no-unsanitized"],
    rules: {
      "no-unsanitized/method": "error",
      "no-unsanitized/property": "error",
    },
  },
  fmt: {
    singleQuote: true,
    printWidth: 120,
  },
});
