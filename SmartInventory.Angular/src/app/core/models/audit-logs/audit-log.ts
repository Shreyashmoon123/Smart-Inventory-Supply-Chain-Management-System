
export interface AuditLog {
  auditLogId: number;
  userId: number | null;
  userName: string | null;
  action: string;
  entityName: string | null;
  entityId: string | null;
  details: string | null;
  createdAt: string;
}

export interface CreateAuditLog {
  action: string;
  entityName?: string | null;
  entityId?: string | null;
  details?: string | null;
}
