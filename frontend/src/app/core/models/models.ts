export enum UserRole {
  Employee = 0,
  Manager = 1
}

export enum TaskStatus {
  New = 0,
  InProgress = 1,
  OnHold = 2,
  Completed = 3,
  Cancelled = 4
}

export enum TaskPriority {
  Low = 0,
  Medium = 1,
  High = 2,
  Critical = 3
}

export interface UserDto {
  id: number;
  fullName: string;
  email: string;
  role: UserRole;
  isActive: boolean;
}

export interface AuthResponse {
  token: string;
  expiresAt: string;
  user: UserDto;
}

export interface TaskDto {
  id: number;
  title: string;
  description?: string | null;
  status: TaskStatus;
  priority: TaskPriority;
  progressPercent: number;
  startDate?: string | null;
  dueDate?: string | null;
  completedDate?: string | null;
  estimatedHours?: number | null;
  actualHours?: number | null;
  notes?: string | null;
  assignedToUserId?: number | null;
  assignedToName?: string | null;
  createdByUserId: number;
  createdAt: string;
  updatedAt: string;
}

/** صف في الجدول القابل للتحرير (شبيه الإكسل) */
export interface TaskUpsert {
  id?: number | null;
  title: string;
  description?: string | null;
  status: TaskStatus;
  priority: TaskPriority;
  progressPercent: number;
  startDate?: string | null;
  dueDate?: string | null;
  completedDate?: string | null;
  estimatedHours?: number | null;
  actualHours?: number | null;
  notes?: string | null;
  assignedToUserId?: number | null;
}

export interface BulkSaveRequest {
  tasks: TaskUpsert[];
  deletedIds: number[];
}

export const STATUS_LABELS: Record<TaskStatus, string> = {
  [TaskStatus.New]: 'جديدة',
  [TaskStatus.InProgress]: 'قيد التنفيذ',
  [TaskStatus.OnHold]: 'معلّقة',
  [TaskStatus.Completed]: 'مكتملة',
  [TaskStatus.Cancelled]: 'ملغاة'
};

export const PRIORITY_LABELS: Record<TaskPriority, string> = {
  [TaskPriority.Low]: 'منخفضة',
  [TaskPriority.Medium]: 'متوسطة',
  [TaskPriority.High]: 'عالية',
  [TaskPriority.Critical]: 'حرجة'
};

export const ROLE_LABELS: Record<UserRole, string> = {
  [UserRole.Employee]: 'موظف',
  [UserRole.Manager]: 'مدير'
};
