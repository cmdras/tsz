import { useEffect, useRef, useState } from 'react';
import { cn } from '#/lib/utils';
import { parseHourInput } from '#/features/time-entries/parse-hour-input';

interface HourCellProps {
  value: number | null;
  dailyOtherTotal: number;
  isWeekend: boolean;
  onCommit: (value: number | null) => void;
}

export function HourCell({ value, dailyOtherTotal, isWeekend, onCommit }: HourCellProps) {
  const [rawInput, setRawInput] = useState('');
  const [isFocused, setIsFocused] = useState(false);
  const [isOver24, setIsOver24] = useState(false);
  const inputRef = useRef<HTMLInputElement>(null);
  const hotkeyCommittedRef = useRef(false);

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

  function triggerFocus() {
    setRawInput(value != null ? String(value) : '');
    setIsOver24(false);
    setIsFocused(true);
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
      hotkeyCommittedRef.current = true;
      onCommit(hotkeyValue);
      inputRef.current?.blur();
    } else if (key === 'delete' || key === 'backspace') {
      event.preventDefault();
      hotkeyCommittedRef.current = true;
      onCommit(null);
      inputRef.current?.blur();
    }
  }

  function handleChange(event: React.ChangeEvent<HTMLInputElement>) {
    const newRaw = event.target.value.replaceAll(',', '.');
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
          className="flex h-8 w-full items-center justify-center rounded text-sm font-bold hover:bg-accent"
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
}
