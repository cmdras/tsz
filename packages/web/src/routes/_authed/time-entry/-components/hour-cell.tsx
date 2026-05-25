import { forwardRef, useEffect, useImperativeHandle, useRef, useState } from 'react';
import { cn } from '#/lib/utils';
import { parseHourInput } from '#/features/time-entries/parse-hour-input';

interface HourCellProps {
  value: number | null;
  dailyOtherTotal: number;
  isWeekend: boolean;
  isLeave?: boolean;
  onCommit: (value: number | null) => void;
  onFocusNext?: () => void;
  onFocusPrev?: () => void;
}

export interface HourCellHandle {
  triggerFocus: () => void;
}

interface UseHourCellParams {
  value: number | null;
  dailyOtherTotal: number;
  onCommit: (value: number | null) => void;
  onFocusNext?: () => void;
  onFocusPrev?: () => void;
}

function useHourCell({ value, dailyOtherTotal, onCommit, onFocusNext, onFocusPrev }: UseHourCellParams) {
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

  useEffect(() => {
    if (isFocused) {
      inputRef.current?.focus();
    }
  }, [isFocused]);

  function commitAndAdvance(newValue: number | null, moveFocus?: () => void) {
    hotkeyCommittedRef.current = true;
    onCommit(newValue);
    moveFocus?.();
    inputRef.current?.blur();
  }

  function resolveCommitValue(): number | null {
    if (isOver24) return value;
    const parsed = parseHourInput(rawInput);
    return parsed === null || parsed === 0 ? null : parsed;
  }

  function handleBlur() {
    setIsFocused(false);
    if (hotkeyCommittedRef.current) {
      hotkeyCommittedRef.current = false;
      return;
    }
    onCommit(resolveCommitValue());
    setIsOver24(false);
  }

  function handleHotkeyChar(hotkeyValue: number) {
    if (dailyOtherTotal + hotkeyValue > 24) {
      setRawInput(String(hotkeyValue));
      setIsOver24(true);
    } else {
      commitAndAdvance(hotkeyValue, onFocusNext);
    }
  }

  function handleEnterKey() {
    commitAndAdvance(resolveCommitValue(), onFocusNext);
  }

  function handleKeyDown(event: React.KeyboardEvent<HTMLInputElement>) {
    const key = event.key.toLowerCase();
    const hotkeyValues: Record<string, number> = { d: 8, h: 4 };
    const hotkeyValue = hotkeyValues[key];

    if (hotkeyValue !== undefined) {
      event.preventDefault();
      handleHotkeyChar(hotkeyValue);
      return;
    }

    const specialKeyHandlers: Record<string, () => void> = {
      enter: handleEnterKey,
      delete: () => commitAndAdvance(null, onFocusPrev),
    };
    const specialHandler = specialKeyHandlers[key];
    if (specialHandler) {
      event.preventDefault();
      specialHandler();
    }
  }

  function handleChange(event: React.ChangeEvent<HTMLInputElement>) {
    // Only allow digits, decimal dot, and comma (comma is normalized to dot below)
    const filtered = event.target.value.replace(/[^\d.,]/g, '');
    const newRaw = filtered.replaceAll(',', '.');
    const numericValue = Number(newRaw);
    setIsOver24(!Number.isNaN(numericValue) && dailyOtherTotal + numericValue > 24);
    setRawInput(newRaw);
  }

  return { rawInput, isFocused, isOver24, inputRef, triggerFocus, handleBlur, handleKeyDown, handleChange };
}

export const HourCell = forwardRef<HourCellHandle, HourCellProps>(function HourCell(
  { value, dailyOtherTotal, isWeekend, isLeave = false, onCommit, onFocusNext, onFocusPrev },
  ref,
) {
  const { rawInput, isFocused, isOver24, inputRef, triggerFocus, handleBlur, handleKeyDown, handleChange } =
    useHourCell({ value, dailyOtherTotal, onCommit, onFocusNext, onFocusPrev });

  // Expose triggerFocus so WeekGrid can programmatically focus a specific cell.
  useImperativeHandle(ref, () => ({ triggerFocus }));

  if (isWeekend) {
    return (
      <div className="border-l p-2 bg-muted/40">
        <div className="flex h-8 items-center justify-center text-muted-foreground/30 select-none">—</div>
      </div>
    );
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
          className={cn(
            'flex h-8 w-full items-center justify-center rounded text-sm font-bold hover:bg-accent',
            isLeave ? 'text-amber-500' : 'text-primary',
          )}
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
