import { describe, expect, it } from 'vitest';
import { buildHomeViewModel } from './home-view';
import type { HomeViewModel } from './home-view';

// ---------------------------------------------------------------------------
// Test helpers
// ---------------------------------------------------------------------------

function makeDay(date: string, totalHours: number, isInMonth = true) {
  return { date, isInMonth, totalHours, entries: [] };
}

function makeWeekSubmission(weekStart: string) {
  return { weekStart, submittedAt: '2026-05-01T10:00:00Z' };
}

function makeLeaveType(id: string, name: string, mode: 'Limited' | 'Unlimited', allowance: number, takenDays: number) {
  return { id, name, mode, allowance, takenDays };
}

function makeLeaveOverview(types: ReturnType<typeof makeLeaveType>[]) {
  return { year: 2026, types, days: [] };
}

/**
 * Builds a minimal MonthResponse for May 2026 (2026-05).
 * The month contains 5 ISO weeks:
 *   W18: Mon 2026-04-27 – Sun 2026-05-03
 *   W19: Mon 2026-05-04 – Sun 2026-05-10
 *   W20: Mon 2026-05-11 – Sun 2026-05-17
 *   W21: Mon 2026-05-18 – Sun 2026-05-24
 *   W22: Mon 2026-05-25 – Sun 2026-05-31
 */
function makeMay2026Month(
  days: ReturnType<typeof makeDay>[],
  weekSubmissions: ReturnType<typeof makeWeekSubmission>[] = [],
) {
  return {
    yearMonth: '2026-05',
    fromDate: '2026-05-01',
    toDate: '2026-05-31',
    days,
    weekSubmissions,
  };
}

const TODAY_MID_MAY = new Date(2026, 4, 14); // 2026-05-14 (Thursday, in W20)
const LEAVE_OVERVIEW_EMPTY = makeLeaveOverview([]);

// ---------------------------------------------------------------------------
// Scenario: all weeks submitted → caughtUp tone
// ---------------------------------------------------------------------------

describe('all-submitted month', () => {
  const allSubmissions = [
    makeWeekSubmission('2026-04-27'),
    makeWeekSubmission('2026-05-04'),
    makeWeekSubmission('2026-05-11'),
    makeWeekSubmission('2026-05-18'),
    makeWeekSubmission('2026-05-25'),
  ];

  const days = [
    makeDay('2026-05-01', 8),
    makeDay('2026-05-02', 8),
    makeDay('2026-05-05', 8),
    makeDay('2026-05-06', 8),
    makeDay('2026-05-07', 8),
    makeDay('2026-05-08', 8),
    makeDay('2026-05-09', 8),
  ];

  const month = makeMay2026Month(days, allSubmissions);
  const leaveOverview = makeLeaveOverview([makeLeaveType('lt1', 'Annual Leave', 'Limited', 20, 5)]);

  let viewModel: HomeViewModel;

  it('produces caughtUp tone', () => {
    viewModel = buildHomeViewModel(month, leaveOverview, TODAY_MID_MAY, 'Alice');
    expect(viewModel.tone).toBe('caughtUp');
  });

  it('has no tasks', () => {
    viewModel = buildHomeViewModel(month, leaveOverview, TODAY_MID_MAY, 'Alice');
    expect(viewModel.tasks).toHaveLength(0);
  });

  it('reports correct weeksSubmitted and weeksTotal', () => {
    viewModel = buildHomeViewModel(month, leaveOverview, TODAY_MID_MAY, 'Alice');
    expect(viewModel.stats.weeksSubmitted).toBe(5);
    expect(viewModel.stats.weeksTotal).toBe(5);
  });

  it('reports correct loggedThisMonth', () => {
    viewModel = buildHomeViewModel(month, leaveOverview, TODAY_MID_MAY, 'Alice');
    expect(viewModel.stats.loggedThisMonth).toBe(56); // 7 days × 8h
  });

  it('reports correct leaveDaysLeft', () => {
    viewModel = buildHomeViewModel(month, leaveOverview, TODAY_MID_MAY, 'Alice');
    expect(viewModel.stats.leaveDaysLeft).toBe(15); // max(0, 20 - 5)
  });

  it('passes through the greetingName', () => {
    viewModel = buildHomeViewModel(month, leaveOverview, TODAY_MID_MAY, 'Alice');
    expect(viewModel.greetingName).toBe('Alice');
  });
});

// ---------------------------------------------------------------------------
// Scenario: mixed month → tasks tone, only unsubmitted weeks
// ---------------------------------------------------------------------------

describe('mixed month (some weeks submitted, some not)', () => {
  // W18 (04-27) and W19 (05-04) submitted; W20, W21, W22 not submitted
  const partialSubmissions = [makeWeekSubmission('2026-04-27'), makeWeekSubmission('2026-05-04')];

  const days = [
    // W19 days (submitted) — should not appear as tasks
    makeDay('2026-05-05', 8),
    makeDay('2026-05-06', 8),
    // W20 days (not submitted) — some hours logged
    makeDay('2026-05-11', 4),
    makeDay('2026-05-12', 4),
    // W21 days (not submitted) — no hours
    makeDay('2026-05-18', 0),
    // W22 days (not submitted) — some hours
    makeDay('2026-05-25', 6),
  ];

  const month = makeMay2026Month(days, partialSubmissions);

  it('produces tasks tone', () => {
    const viewModel = buildHomeViewModel(month, LEAVE_OVERVIEW_EMPTY, TODAY_MID_MAY, 'Bob');
    expect(viewModel.tone).toBe('tasks');
  });

  it('includes only unsubmitted weeks as tasks', () => {
    const viewModel = buildHomeViewModel(month, LEAVE_OVERVIEW_EMPTY, TODAY_MID_MAY, 'Bob');
    const weekStarts = viewModel.tasks.map((task) => task.weekStart);
    expect(weekStarts).toEqual(['2026-05-11', '2026-05-18', '2026-05-25']);
  });

  it('tasks are in chronological order (oldest first)', () => {
    const viewModel = buildHomeViewModel(month, LEAVE_OVERVIEW_EMPTY, TODAY_MID_MAY, 'Bob');
    const weekStarts = viewModel.tasks.map((task) => task.weekStart);
    const sorted = [...weekStarts].toSorted();
    expect(weekStarts).toEqual(sorted);
  });

  it('stats.weeksSubmitted reflects submitted count', () => {
    const viewModel = buildHomeViewModel(month, LEAVE_OVERVIEW_EMPTY, TODAY_MID_MAY, 'Bob');
    expect(viewModel.stats.weeksSubmitted).toBe(2);
    expect(viewModel.stats.weeksTotal).toBe(5);
  });
});

// ---------------------------------------------------------------------------
// Scenario: isPrimary — week containing today is the sole primary task
// ---------------------------------------------------------------------------

describe('isPrimary', () => {
  const noSubmissions: ReturnType<typeof makeWeekSubmission>[] = [];

  const days = [
    makeDay('2026-05-04', 2), // W19
    makeDay('2026-05-11', 2), // W20
    makeDay('2026-05-18', 2), // W21
    makeDay('2026-05-25', 2), // W22
  ];

  const month = makeMay2026Month(days, noSubmissions);

  it('marks only the task containing today as isPrimary', () => {
    // today is 2026-05-14 (Thursday) → W20 Monday = 2026-05-11
    const viewModel = buildHomeViewModel(month, LEAVE_OVERVIEW_EMPTY, TODAY_MID_MAY, 'Carol');
    const primaryTasks = viewModel.tasks.filter((task) => task.isPrimary);
    expect(primaryTasks).toHaveLength(1);
    expect(primaryTasks[0]!.weekStart).toBe('2026-05-11');
  });

  it('no task has isPrimary when today falls on a submitted week', () => {
    const submittedThisWeek = [makeWeekSubmission('2026-05-11')];
    const monthWithSubmission = makeMay2026Month(days, submittedThisWeek);
    const viewModel = buildHomeViewModel(monthWithSubmission, LEAVE_OVERVIEW_EMPTY, TODAY_MID_MAY, 'Carol');
    const primaryTasks = viewModel.tasks.filter((task) => task.isPrimary);
    expect(primaryTasks).toHaveLength(0);
  });
});

// ---------------------------------------------------------------------------
// Scenario: task status — empty vs draft
// ---------------------------------------------------------------------------

describe('task status', () => {
  const noSubmissions: ReturnType<typeof makeWeekSubmission>[] = [];

  it('status is empty when loggedHours === 0', () => {
    const days = [makeDay('2026-05-11', 0), makeDay('2026-05-12', 0)];
    const month = makeMay2026Month(days, noSubmissions);
    const viewModel = buildHomeViewModel(month, LEAVE_OVERVIEW_EMPTY, TODAY_MID_MAY, 'Dave');
    const w20 = viewModel.tasks.find((task) => task.weekStart === '2026-05-11');
    expect(w20).toBeDefined();
    expect(w20!.status).toBe('empty');
    expect(w20!.loggedHours).toBe(0);
  });

  it('status is draft when loggedHours > 0', () => {
    const days = [makeDay('2026-05-11', 3), makeDay('2026-05-12', 5)];
    const month = makeMay2026Month(days, noSubmissions);
    const viewModel = buildHomeViewModel(month, LEAVE_OVERVIEW_EMPTY, TODAY_MID_MAY, 'Dave');
    const w20 = viewModel.tasks.find((task) => task.weekStart === '2026-05-11');
    expect(w20).toBeDefined();
    expect(w20!.status).toBe('draft');
    expect(w20!.loggedHours).toBe(8);
  });
});

// ---------------------------------------------------------------------------
// Scenario: month-boundary week — only in-month days count toward hours
// ---------------------------------------------------------------------------

describe('month-boundary week (W18: Mon 2026-04-27 overlaps into May)', () => {
  const noSubmissions: ReturnType<typeof makeWeekSubmission>[] = [];

  it('counts only in-month days (May 1-3) toward loggedHours for W18', () => {
    // Only May days are in-month (isInMonth=true); April days not in the month response
    const days = [
      // April days — should NOT be returned for the May endpoint; simulate with isInMonth=false
      makeDay('2026-04-27', 8, false),
      makeDay('2026-04-28', 8, false),
      makeDay('2026-04-29', 8, false),
      makeDay('2026-04-30', 8, false),
      // May days
      makeDay('2026-05-01', 6, true),
      makeDay('2026-05-02', 6, true),
      makeDay('2026-05-03', 6, true),
    ];

    const month = makeMay2026Month(days, noSubmissions);
    const viewModel = buildHomeViewModel(month, LEAVE_OVERVIEW_EMPTY, new Date(2026, 4, 1), 'Eve');

    const w18 = viewModel.tasks.find((task) => task.weekStart === '2026-04-27');
    expect(w18).toBeDefined();
    // Only May 1-3 are in-month → 18h
    expect(w18!.loggedHours).toBe(18);
  });

  it('W18 is included as a task even though its Monday is in April', () => {
    const days = [makeDay('2026-05-01', 0), makeDay('2026-05-02', 0), makeDay('2026-05-03', 0)];
    const month = makeMay2026Month(days, noSubmissions);
    const viewModel = buildHomeViewModel(month, LEAVE_OVERVIEW_EMPTY, new Date(2026, 4, 1), 'Eve');

    const weekStarts = viewModel.tasks.map((task) => task.weekStart);
    expect(weekStarts).toContain('2026-04-27');
  });

  it('loggedThisMonth sums only isInMonth days', () => {
    const days = [makeDay('2026-04-27', 8, false), makeDay('2026-05-01', 6, true), makeDay('2026-05-02', 6, true)];
    const month = makeMay2026Month(days, noSubmissions);
    const viewModel = buildHomeViewModel(month, LEAVE_OVERVIEW_EMPTY, new Date(2026, 4, 1), 'Eve');
    // Only the two May days → 12h
    expect(viewModel.stats.loggedThisMonth).toBe(12);
  });
});

// ---------------------------------------------------------------------------
// Scenario: leaveDaysLeft computation
// ---------------------------------------------------------------------------

describe('leaveDaysLeft', () => {
  const month = makeMay2026Month([], []);

  it('sums max(0, allowance − takenDays) over Limited types only', () => {
    const types = [
      makeLeaveType('lt1', 'Annual Leave', 'Limited', 20, 5), // 15 left
      makeLeaveType('lt2', 'Sick Leave', 'Limited', 10, 3), // 7 left
      makeLeaveType('lt3', 'Unlimited PTO', 'Unlimited', 0, 2), // ignored
    ];
    const leaveOverview = makeLeaveOverview(types);
    const viewModel = buildHomeViewModel(month, leaveOverview, TODAY_MID_MAY, 'Frank');
    expect(viewModel.stats.leaveDaysLeft).toBe(22); // 15 + 7
  });

  it('ignores Unlimited leave types', () => {
    const types = [makeLeaveType('lt1', 'Unlimited PTO', 'Unlimited', 999, 0)];
    const leaveOverview = makeLeaveOverview(types);
    const viewModel = buildHomeViewModel(month, leaveOverview, TODAY_MID_MAY, 'Frank');
    expect(viewModel.stats.leaveDaysLeft).toBe(0);
  });

  it('clamps negative balances to 0 (takenDays exceeds allowance)', () => {
    const types = [
      makeLeaveType('lt1', 'Annual Leave', 'Limited', 5, 10), // would be -5 → clamped to 0
      makeLeaveType('lt2', 'Sick Leave', 'Limited', 10, 2), // 8 left
    ];
    const leaveOverview = makeLeaveOverview(types);
    const viewModel = buildHomeViewModel(month, leaveOverview, TODAY_MID_MAY, 'Frank');
    expect(viewModel.stats.leaveDaysLeft).toBe(8); // 0 + 8
  });

  it('returns 0 when no leave types exist', () => {
    const leaveOverview = makeLeaveOverview([]);
    const viewModel = buildHomeViewModel(month, leaveOverview, TODAY_MID_MAY, 'Frank');
    expect(viewModel.stats.leaveDaysLeft).toBe(0);
  });
});

// ---------------------------------------------------------------------------
// Scenario: weekNumber is correct ISO week number
// ---------------------------------------------------------------------------

describe('weekNumber', () => {
  it('assigns correct ISO week numbers to tasks', () => {
    const noSubmissions: ReturnType<typeof makeWeekSubmission>[] = [];
    const days = [makeDay('2026-05-11', 0), makeDay('2026-05-18', 0), makeDay('2026-05-25', 0)];
    const month = makeMay2026Month(days, noSubmissions);
    const viewModel = buildHomeViewModel(month, LEAVE_OVERVIEW_EMPTY, TODAY_MID_MAY, 'Grace');

    const w20 = viewModel.tasks.find((task) => task.weekStart === '2026-05-11');
    expect(w20?.weekNumber).toBe(20);

    const w21 = viewModel.tasks.find((task) => task.weekStart === '2026-05-18');
    expect(w21?.weekNumber).toBe(21);

    const w22 = viewModel.tasks.find((task) => task.weekStart === '2026-05-25');
    expect(w22?.weekNumber).toBe(22);
  });
});
