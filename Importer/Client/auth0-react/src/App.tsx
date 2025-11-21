import { useState, useEffect } from 'react'
import { useAuth0 } from '@auth0/auth0-react'
import reactLogo from './assets/react.svg'
import viteLogo from '/vite.svg'
import './App.css'

function App() {
  const [count, setCount] = useState(0)
  const { user, isAuthenticated, isLoading, loginWithRedirect, error, logout, getAccessTokenSilently } = useAuth0()

  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      loginWithRedirect().then((data) => {
        console.log('Redirecting to login...', data)
      }).catch((e) => {
        console.error('Error during login redirect:', e)
      })
    }

    getAccessTokenSilently().then((token) => {
      console.log('Access Token:', token)
    }).catch((e) => {
      console.error('Error getting access token:', e)
    })
  }, [isLoading, isAuthenticated, loginWithRedirect, getAccessTokenSilently])

  if (error) {
    return <div>Authentication Error: {error.message}</div>
  }

  if (isLoading) {
    return <div>Loading...</div>
  }

  if (!isAuthenticated) {
    return null
  }

  return (
    <>
      <div>
        <button onClick={() => logout({ logoutParams: { returnTo: window.location.origin } })} style={{ marginRight: '1rem' }}>
          Logout
        </button>
        <a href="https://vite.dev" target="_blank">
          <img src={viteLogo} className="logo" alt="Vite logo" />
        </a>
        <a href="https://react.dev" target="_blank">
          <img src={reactLogo} className="logo react" alt="React logo" />
        </a>
      </div>
      <h1>Vite + React (Protected)</h1>
      <h1>Welcome, {user?.name}</h1>
      <div className="card">
        <button onClick={() => setCount((count) => count + 1)}>
          count is {count}
        </button>
        <p>
          Edit <code>src/App.tsx</code> and save to test HMR
        </p>
      </div>
      <p className="read-the-docs">
        Click on the Vite and React logos to learn more
      </p>
    </>
  )
}

export default App
