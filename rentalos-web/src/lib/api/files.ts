import api from './client';

export interface PresignFileRequest {
  fileName: string;
  contentType: string;
  size: number;
}

export interface DeleteFileRequest {
  key: string;
}

export const filesApi = {
  presign: (data: PresignFileRequest) => api.post('/files/presign', data),
  delete: (data: DeleteFileRequest) => api.delete('/files', { data }),
};
