import { z } from 'zod';

const envSchema = z.object({
  SERVER_URL: z.url(),
  APP_BASE_URL: z.url().default('http://localhost:3000'),
  AUTH_TENANT_ID: z.string().min(1),
  AUTH_CLIENT_ID: z.string().min(1),
  AUTH_CLIENT_SECRET: z.string().min(1),
  AUTH_SECRET: z.string().min(1),
});

const result = envSchema.safeParse(process.env);

if (!result.success) {
  const issues = result.error.issues.map((i) => `  ${i.path.join('.')}: ${i.message}`).join('\n');
  console.error(`\n[env] Invalid environment variables:\n${issues}\n`);
  process.exit(1);
}

export const env = result.data;
