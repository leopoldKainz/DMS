export function saveFile(url: string, fileName: string) {
  const link = document.createElement('a')
  link.href = url
  link.download = fileName
  document.body.append(link)
  link.click()
  link.remove()
}

export function saveBlob(blob: Blob, fileName: string) {
  const url = URL.createObjectURL(blob)
  saveFile(url, fileName)
  setTimeout(() => URL.revokeObjectURL(url), 1000)
}
