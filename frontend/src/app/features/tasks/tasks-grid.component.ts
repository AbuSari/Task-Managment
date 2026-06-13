import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TaskService } from '../../core/services/task.service';
import { UserService } from '../../core/services/user.service';
import { AuthService } from '../../core/services/auth.service';
import {
  PRIORITY_LABELS, STATUS_LABELS, TaskDto, TaskPriority, TaskStatus, TaskUpsert, UserDto
} from '../../core/models/models';

/** صف قابل للتحرير في الجدول */
interface EditableRow extends TaskUpsert {
  _clientId: number;
  _dirty: boolean;
}

@Component({
  selector: 'app-tasks-grid',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './tasks-grid.component.html',
  styleUrl: './tasks-grid.component.scss'
})
export class TasksGridComponent implements OnInit {
  private taskService = inject(TaskService);
  private userService = inject(UserService);
  private auth = inject(AuthService);

  readonly isManager = this.auth.isManager;

  rows = signal<EditableRow[]>([]);
  users = signal<UserDto[]>([]);
  deletedIds = signal<number[]>([]);

  loading = signal(false);
  saving = signal(false);
  message = signal<{ type: 'success' | 'error'; text: string } | null>(null);
  search = signal('');

  private nextClientId = 1;

  // خيارات القوائم المنسدلة
  readonly statusOptions = Object.values(TaskStatus).filter(v => typeof v === 'number') as TaskStatus[];
  readonly priorityOptions = Object.values(TaskPriority).filter(v => typeof v === 'number') as TaskPriority[];
  readonly statusLabels = STATUS_LABELS;
  readonly priorityLabels = PRIORITY_LABELS;

  readonly hasChanges = computed(() =>
    this.deletedIds().length > 0 || this.rows().some(r => r._dirty || !r.id));

  readonly filteredRows = computed(() => {
    const q = this.search().trim().toLowerCase();
    if (!q) return this.rows();
    return this.rows().filter(r =>
      (r.title ?? '').toLowerCase().includes(q) ||
      (r.description ?? '').toLowerCase().includes(q) ||
      (r.notes ?? '').toLowerCase().includes(q));
  });

  ngOnInit(): void {
    if (this.isManager()) {
      this.userService.getAll().subscribe(u => this.users.set(u));
    }
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.taskService.getAll().subscribe({
      next: tasks => {
        this.rows.set(tasks.map(t => this.toRow(t)));
        this.deletedIds.set([]);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.notify('error', 'تعذّر تحميل المهام');
      }
    });
  }

  addRow(): void {
    const row: EditableRow = {
      _clientId: this.nextClientId++,
      _dirty: true,
      id: null,
      title: '',
      description: '',
      status: TaskStatus.New,
      priority: TaskPriority.Medium,
      progressPercent: 0,
      startDate: null,
      dueDate: null,
      estimatedHours: null,
      actualHours: null,
      notes: '',
      assignedToUserId: null
    };
    this.rows.update(rows => [row, ...rows]);
  }

  markDirty(row: EditableRow): void {
    row._dirty = true;
  }

  removeRow(row: EditableRow): void {
    if (row.id) {
      this.deletedIds.update(ids => [...ids, row.id as number]);
    }
    this.rows.update(rows => rows.filter(r => r._clientId !== row._clientId));
  }

  save(): void {
    // التحقق: العنوان مطلوب
    const invalid = this.rows().find(r => !r.title?.trim());
    if (invalid) {
      this.notify('error', 'العنوان مطلوب في جميع الصفوف');
      return;
    }

    this.saving.set(true);
    const payload = {
      tasks: this.rows().map(r => this.toUpsert(r)),
      deletedIds: this.deletedIds()
    };

    this.taskService.bulkSave(payload).subscribe({
      next: tasks => {
        this.rows.set(tasks.map(t => this.toRow(t)));
        this.deletedIds.set([]);
        this.saving.set(false);
        this.notify('success', 'تم حفظ التغييرات بنجاح');
      },
      error: () => {
        this.saving.set(false);
        this.notify('error', 'فشل الحفظ، تحقق من البيانات');
      }
    });
  }

  // ----- Excel -----
  exportExcel(): void {
    this.taskService.exportExcel().subscribe(blob =>
      this.download(blob, `tasks_${new Date().toISOString().slice(0, 10)}.xlsx`));
  }

  downloadTemplate(): void {
    this.taskService.downloadTemplate().subscribe(blob =>
      this.download(blob, 'tasks_template.xlsx'));
  }

  importExcel(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    this.loading.set(true);
    this.taskService.importExcel(file).subscribe({
      next: res => {
        this.notify('success', `تم استيراد ${res.imported} مهمة`);
        this.load();
      },
      error: () => {
        this.loading.set(false);
        this.notify('error', 'فشل استيراد الملف');
      }
    });
    input.value = '';
  }

  userName(id: number | null | undefined): string {
    if (!id) return '—';
    return this.users().find(u => u.id === id)?.fullName ?? '—';
  }

  trackByClientId = (_: number, row: EditableRow) => row._clientId;

  // ----- أدوات داخلية -----
  private toRow(t: TaskDto): EditableRow {
    return {
      _clientId: this.nextClientId++,
      _dirty: false,
      id: t.id,
      title: t.title,
      description: t.description,
      status: t.status,
      priority: t.priority,
      progressPercent: t.progressPercent,
      startDate: this.toDateInput(t.startDate),
      dueDate: this.toDateInput(t.dueDate),
      estimatedHours: t.estimatedHours,
      actualHours: t.actualHours,
      notes: t.notes,
      assignedToUserId: t.assignedToUserId
    };
  }

  private toUpsert(r: EditableRow): TaskUpsert {
    return {
      id: r.id ?? null,
      title: r.title?.trim() ?? '',
      description: r.description,
      status: Number(r.status),
      priority: Number(r.priority),
      progressPercent: Number(r.progressPercent) || 0,
      startDate: r.startDate || null,
      dueDate: r.dueDate || null,
      estimatedHours: r.estimatedHours != null && r.estimatedHours !== ('' as any)
        ? Number(r.estimatedHours) : null,
      actualHours: r.actualHours != null && r.actualHours !== ('' as any)
        ? Number(r.actualHours) : null,
      notes: r.notes,
      assignedToUserId: r.assignedToUserId ? Number(r.assignedToUserId) : null
    };
  }

  private toDateInput(value?: string | null): string | null {
    return value ? value.slice(0, 10) : null;
  }

  private download(blob: Blob, fileName: string): void {
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = fileName;
    a.click();
    window.URL.revokeObjectURL(url);
  }

  private notify(type: 'success' | 'error', text: string): void {
    this.message.set({ type, text });
    setTimeout(() => this.message.set(null), 4000);
  }
}
