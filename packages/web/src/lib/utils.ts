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

const avatarPalette = [
  '#014046', // Euricom Midnight
  '#1e3a5f', // deep steel blue
  '#3d1f5f', // deep purple
  '#5c2d1e', // deep rust
  '#1e4d3d', // deep forest
  '#4d1e3d', // deep rose
  '#3a3d1e', // deep olive
  '#1e3d52', // deep slate
];

export function getAvatarColor(name: string): string {
  const hash = [...name].reduce((accumulator, character) => accumulator + character.charCodeAt(0), 0);
  return avatarPalette[hash % avatarPalette.length];
}
