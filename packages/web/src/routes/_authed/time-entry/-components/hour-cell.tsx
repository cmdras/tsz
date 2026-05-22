import { forwardRef, useEffect, useImperativeHandle, useRef, useState } from 'react';
import { cn } from '#/lib/utils';
import { parseHourInput } from '#/features/time-entries/parse-hour-input';

interface HourCellProps {
  value: number | null;
  dailyOtherTotal: number;
  isWeekend: boolean;
  onCommit: (value: number | null) => void;
  onFocusNext?: () => void;
  onFocusPrev?: () => void;
}

export interface HourCellHandle {
  triggerFocus: () => void;
}

export const HourCell = forwardRef<HourCellHandle, HourCellProps>(function HourCell(
  { value, dailyOtherTotal, isWeekend, onCommit, onFocusNext, onFocusPrev },
  ref,
) {
  const [rawInput, setRawInput] = useState('');
  const [isFocused, setIsFocused] = useState(false);
  const [isOver24, setIsOver24] = useState(false);
  const inputRef = useRef<HTMLInputElement>(null);
  const hotkeyCommittedRef = useRef(false);

  function triggerFocus() {
    setRawInput(value != null ? String(value) : '');
    setIsOver24(false);
    setIsFocused(true);
  }

  // Expose triggerFocus so WeekGrid can programmatically focus a specific cell.
  useImperativeHandle(ref, () => ({ triggerFocus }));

  useEffect(() => {
    if (isFocused) {
      inputRef.current?.focus();
    }
  }, [isFocused]);

  if (isWeekend) {
    return (
      <div className="border-l p-2 bg-muted/40">
        <div className="flex h-8 items-center justify-center text-muted-foreground/30 select-none">—</div>
      </div>
    );
  }

  function commitAndAdvance(newValue: number | null, direction: 'next' | 'prev' | 'none') {
    hotkeyCommittedRef.current = true;
    onCommit(newValue);
    if (direction === 'next') onFocusNext?.();
    else if (direction === 'prev') onFocusPrev?.();
    inputRef.current?.blur();
  }

  function handleBlur() {
    setIsFocused(false);
    if (hotkeyCommittedRef.current) {
      hotkeyCommittedRef.current = false;
      return;
    }
    const parsed = parseHourInput(rawInput);
    if (isOver24) {
      onCommit(value); // revert to saved value rather than clearing
    } else if (parsed === null || parsed === 0) {
      onCommit(null);
    } else {
      onCommit(parsed);
    }
    setIsOver24(false);
  }

  function handleKeyDown(event: React.KeyboardEvent<HTMLInputElement>) {
    const key = event.key.toLowerCase();
    const hotkeyValues: Record<string, number> = { d: 8, h: 4 };
    const hotkeyValue = hotkeyValues[key];

    if (hotkeyValue !== undefined) {
      event.preventDefault();
      if (dailyOtherTotal + hotkeyValue > 24) {
        setRawInput(String(hotkeyValue));
        setIsOver24(true);
        return;
      }
      commitAndAdvance(hotkeyValue, 'next');
    } else if (key === 'enter') {
      event.preventDefault();
      const parsed = parseHourInput(rawInput);
      const committed = isOver24 ? value : parsed === null || parsed === 0 ? null : parsed;
      commitAndAdvance(committed, 'next');
    } else if (key === 'delete') {
      event.preventDefault();
      commitAndAdvance(null, 'prev');
    }
    // Backspace: default browser behavior (delete character before cursor)
  }

  function handleChange(event: React.ChangeEvent<HTMLInputElement>) {
    // Only allow digits, decimal dot, and comma (comma is normalized to dot below)
    const filtered = event.target.value.replace(/[^\d.,]/g, '');
    const newRaw = filtered.replaceAll(',', '.');
    const parsed = parseHourInput(newRaw);
    const exceeds = parsed !== null && dailyOtherTotal + parsed > 24;
    setIsOver24(exceeds);
    setRawInput(newRaw);
  }

  if (isFocused) {
    return (
      <div className="border-l p-2">
        <input
          ref={inputRef}
          type="text"
          inputMode="decimal"
          value={rawInput}
          onBlur={handleBlur}
          onKeyDown={handleKeyDown}
          onChange={handleChange}
          className={cn(
            'h-8 w-full rounded border bg-background px-1 text-center text-sm',
            isOver24 ? 'border-destructive bg-destructive/10' : 'border-input',
          )}
        />
      </div>
    );
  }

  return (
    <div className="border-l p-2">
      {value != null ? (
        <button
          type="button"
          onClick={triggerFocus}
          className="flex h-8 w-full items-center justify-center rounded text-sm font-bold text-primary hover:bg-accent"
        >
          {value}h
        </button>
      ) : (
        <button
          type="button"
          onClick={triggerFocus}
          className="flex h-8 w-full items-center justify-center rounded text-muted-foreground/40 hover:bg-accent"
        >
          —
        </button>
      )}
    </div>
  );
});
