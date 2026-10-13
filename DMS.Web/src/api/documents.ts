import { query, request } from './http'

export interface DocumentDto {
  id: string
  name: string
  type: string
  size: number
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

export function deleteDocument(id: string) {
  return request<void>(`/Document/${id}`, { method: 'DELETE' })
}

export function uploadDocuments(files: File[]) {
  const body = new FormData()
  for (const file of files) body.append('files', file)
  return request<DocumentDto[]>('/Document/upload', { method: 'POST', body })
}
