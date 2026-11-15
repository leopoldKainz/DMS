import { query, request, requestBlob } from './http'

export type OcrStatus = 'Pending' | 'Processing' | 'Done' | 'Failed'

export interface DocumentMetadata {
  contentType?: string | null
  pageCount?: number | null
}

export interface DocumentDto {
  id: string
  name: string
  type: string
  size: number
  createdAt: string
  updatedOn?: string | null
  ocrStatus?: OcrStatus
  metadata?: DocumentMetadata
}

export interface DocumentUserDto {
  userId: string
  username: string | null
  permission: 'Owner' | 'User'
  createdAt: string
}

export function listDocuments() {
  return request<DocumentDto[]>('/Document')
}

export function searchDocuments(term: string) {
  return request<DocumentDto[]>(`/Document/search${query({ query: term })}`)
}

export function getDocument(id: string) {
  return request<DocumentDto>(`/Document/${id}`)
}

export function getDocumentUsers(id: string, signal?: AbortSignal) {
  return request<DocumentUserDto[]>(`/Document/${id}/users`, { signal })
}

export function fetchDocumentFile(id: string, signal?: AbortSignal) {
  return requestBlob(`/Document/${id}/download`, { signal })
}

export function deleteDocument(id: string) {
  return request<void>(`/Document/${id}`, { method: 'DELETE' })
}

export function uploadDocuments(files: File[]) {
  const body = new FormData()
  for (const file of files) body.append('files', file)
  return request<DocumentDto[]>('/Document/upload', { method: 'POST', body })
}
