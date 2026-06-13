import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { BulkSaveRequest, TaskDto } from '../models/models';

@Injectable({ providedIn: 'root' })
export class TaskService {
  private readonly api = `${environment.apiUrl}/tasks`;
  private readonly excelApi = `${environment.apiUrl}/excel`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<TaskDto[]> {
    return this.http.get<TaskDto[]>(this.api);
  }

  bulkSave(req: BulkSaveRequest): Observable<TaskDto[]> {
    return this.http.post<TaskDto[]>(`${this.api}/bulk-save`, req);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.api}/${id}`);
  }

  // ----- Excel -----
  exportExcel(): Observable<Blob> {
    return this.http.get(`${this.excelApi}/export`, { responseType: 'blob' });
  }

  downloadTemplate(): Observable<Blob> {
    return this.http.get(`${this.excelApi}/template`, { responseType: 'blob' });
  }

  importExcel(file: File): Observable<{ imported: number }> {
    const form = new FormData();
    form.append('file', file);
    return this.http.post<{ imported: number }>(`${this.excelApi}/import`, form);
  }
}
