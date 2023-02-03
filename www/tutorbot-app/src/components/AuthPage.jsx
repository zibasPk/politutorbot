import React from "react"

export default function (props) {
  return (
    <div className="Auth-form-container">
      <form className="Auth-form">
        <div className="Auth-form-content">
          <h3 className="Auth-form-title">Sign In</h3>
          <div className="form-group mt-3">
            <label>Nome Utente</label>
            <input
              type="email"
              className="form-control mt-1"
              placeholder="nome utente"
            />
          </div>
          <div className="form-group mt-3">
            <label>Password</label>
            <input
              type="password"
              className="form-control mt-1"
              placeholder="password"
            />
          </div>
          <div className="d-grid gap-2 mt-3">
            <button onClick={() => props.userStateSetter(true)} className="btn btn-primary">
              Accedi
            </button>
          </div>
        </div>
      </form>
    </div>
  )
}