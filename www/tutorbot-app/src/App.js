import React, { useState } from 'react';
import { HashRouter, Routes, Route, Outlet } from "react-router-dom";
import './index.css';
import 'bootstrap/dist/css/bootstrap.min.css';

import Navigation from './components/NavBar';
import ActiveTutorings from './components/active-tutorings/ActiveTutorings';
import EnabledStudents from './components/enabledStudents/EnabledStudents';
import TutorManagement from './components/tutorManagement/TutorManagement';
import Reservations from './components/reservations/Reservations';
import DataManagement from './components/dataManagement/DataManagement';
import AuthPage from './components/AuthPage';
import NotFound from './components/NotFound';


function App()
{

  const [isUserLogged, setUserLog] = useState(false); 

  const DefaultLayout = () => (
    <>
      <Navigation />
      <Outlet />
    </>
  )

  return (
    <HashRouter>
      <Routes>
        {isUserLogged ?
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
          <Route path="*" element={<AuthPage userStateSetter={(value) => setUserLog(value)}/>} />
        }
      </Routes>

    </HashRouter >

  );
}

export default App;