import { query, request } from './http'

export interface DocumentDto {
  id: number
  name: string
  type: string
  size: number
  createdAt: string
}

export function listDocuments(userId?: number) {
  return request<DocumentDto[]>(`/Document${query({ userId })}`)
}

export function searchDocuments(term: string, userId?: number) {
  return request<DocumentDto[]>(`/Document/search${query({ query: term, userId })}`)
}

export function getDocument(id: string) {
  return request<DocumentDto>(`/Document/${id}`)
}

export function deleteDocument(id: number) {
  return request<void>(`/Document/${id}`, { method: 'DELETE' })
}

export function uploadDocuments(files: File[], userId: number) {
  const body = new FormData()
  for (const file of files) body.append('files', file)
  return request<DocumentDto[]>(`/Document/upload${query({ userId })}`, { method: 'POST', body })
}
