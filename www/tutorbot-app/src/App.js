import React, { useEffect, useState } from 'react';
import { HashRouter, Routes, Route, Outlet } from "react-router-dom";
import './index.css';
import 'bootstrap/dist/css/bootstrap.min.css';

import Cookies from 'universal-cookie';


import Navigation from './components/NavBar';
import ActiveTutorings from './components/active-tutorings/ActiveTutorings';
import EnabledStudents from './components/enabledStudents/EnabledStudents';
import TutorManagement from './components/tutorManagement/TutorManagement';
import Reservations from './components/reservations/Reservations';
import DataManagement from './components/dataManagement/DataManagement';
import AuthPage from './components/AuthPage';
import NotFound from './components/NotFound';
import { makeCall } from './MakeCall';
import configData from './config/config.json';
import NoBackEndConnection from './components/NoBackEndConnection';


function App()
{
  const cookies = new Cookies();

  const [token, setToken] = useState(cookies.get("authToken") !== undefined);

  const [noBackend, setNoBackend] = useState(null);

  const checkBackend = async () =>
  {
    await fetch(configData.botApiUrl + "/", { method: "GET" })
      .then(function (response)
      {
        if (!response.ok)
        {
          setNoBackend(true);
          throw Error(response.statusText);
        }
        setNoBackend(false);
        console.log("Backend is up and running!");
      })
      .catch(function (error)
      {
        console.log("Backend is down:", error);
        setNoBackend(true);
      });
  }

  useEffect(() =>
  {
    setNoBackend(null);
    checkBackend();
  }, []);

  const refresh = () =>
  {
    setToken(cookies.get("authToken") !== undefined)
  }

  const DefaultLayout = () => (
    <>
      <Navigation />
      <Outlet />
    </>
  )

  if (noBackend == null) return <></>;
  if (noBackend) return <NoBackEndConnection />;

  return (
    <HashRouter>
      <Routes>
        {token ?
          <Route path="" element={<DefaultLayout />} >
            <Route path="" element={<Reservations />} />
            <Route path="reservations" element={<Reservations />} />
            <Route path="active-tutorings" element={<ActiveTutorings />} />
            <Route path="enabled-students" element={<EnabledStudents />} />
            <Route path="manage-tutors" element={<TutorManagement />} />
            <Route path="manage-data" element={<DataManagement />} />
            <Route path="*" element={<NotFound />} />
          </Route>
          :
          <Route path="*" element={<AuthPage refresh={refresh} />} />
        }
      </Routes>
    </HashRouter >
  );
}

export default App;