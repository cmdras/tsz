// fallow-ignore-file
import type { ClassValue } from 'clsx';
import { clsx } from 'clsx';
import { twMerge } from 'tailwind-merge';

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

export function formatEntityNumber(number: number): string {
  return String(number).padStart(6, '0');
}
