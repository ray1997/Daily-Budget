export type TransactionType = 'Expense' | 'Income' | 0 | 1;
export type SalaryCycleMode = 'FixedDate' | 'Biweekly' | 'LastWeekdayOfMonth' | 0 | 1 | 2;

export interface TransactionDto {
  id: string;
  amount: number;
  note: string;
  timestamp: string;
  transactionDate: string;
  type: TransactionType;
}

export interface CycleDto {
  id: string;
  startingBalance: number;
  startDate: string;
  nextSalaryDate: string;
  salaryCycleMode: SalaryCycleMode;
  fixedSalaryDay?: number;
  biweeklyAnchorDate?: string;
  lastWeekday?: number;
  currencySymbol: string;
}

export interface AllowancePointDto {
  date: string;
  dailyAllowance: number;
}

export interface DashboardDto {
  cycle: CycleDto | null;
  remainingMoney: number;
  daysUntilSalary: number;
  dailyBudget: number;
  spentToday: number;
  todayProgressPercent: number;
  projectedRemaining: number;
  allowanceHistory: AllowancePointDto[];
  transactions: TransactionDto[];
}
