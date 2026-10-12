import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { createBrowserRouter, RouterProvider } from 'react-router'
import AppShell from './components/AppShell.tsx'
import Dashboard from './pages/Dashboard.tsx'
import DocumentDetail from './pages/DocumentDetail.tsx'
import Login from './pages/Login.tsx'
import Register from './pages/Register.tsx'
import './index.css'

const router = createBrowserRouter([
  { path: '/login', element: <Login /> },
  { path: '/register', element: <Register /> },
  {
    path: '/',
    element: <AppShell />,
    children: [
      { index: true, element: <Dashboard /> },
      { path: 'documents/:id', element: <DocumentDetail /> },
    ],
  },
])

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <RouterProvider router={router} />
  </StrictMode>,
)
