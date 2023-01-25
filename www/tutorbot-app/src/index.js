import React from 'react';
import ReactDOM from 'react-dom/client';
import { BrowserRouter,HashRouter, Routes, Route } from "react-router-dom";
import './index.css';
import 'bootstrap/dist/css/bootstrap.min.css';

import Navigation from './components/NavBar';
import ActiveTutorings from './components/active-tutorings/ActiveTutorings';
import EnabledStudents from './components/enabledStudents/EnabledStudents';
import TutorManagement from './components/tutorManagement/TutorManagement';
import Reservations from './components/reservations/Reservations';
import DataManagement from './components/dataManagement/DataManagement';

const root = ReactDOM.createRoot(document.getElementById('root'));
root.render(
  <>
    <HashRouter>
      <Navigation />
      <div className='page'>
        <Routes>
          <Route path="" element={<Reservations />} />
          <Route path="reservations" element={<Reservations />} />
          <Route path="active-tutorings" element={<ActiveTutorings />} />
          <Route path="enabled-students" element={<EnabledStudents />} />
          <Route path="manage-tutors" element={<TutorManagement />} />
          <Route path="manage-data" element={<DataManagement />} />
        </Routes>
      </div>
    </HashRouter>
    {/* <Footer/> */}
  </>
);
