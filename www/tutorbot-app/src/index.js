import React from 'react';
import ReactDOM from 'react-dom/client';
import { BrowserRouter, Routes, Route } from "react-router-dom";
import './index.css';
import 'bootstrap/dist/css/bootstrap.min.css';

import Navigation from './components/NavBar';
import ActiveTutorings from './components/active-tutorings/ActiveTutorings';
import EnabledStudents from './components/enabledStudents/EnabledStudents';
import TutorManagement from './components/tutorManagement/TutorManagement';
import Reservations from './components/reservations/Reservations';

const root = ReactDOM.createRoot(document.getElementById('root'));
root.render(
  <>
    <BrowserRouter>
      <Navigation />
      <div className='page'>
        <Routes>
          <Route path="/" element={<Reservations />} />
          <Route path="/PoliTutorBot/reservations" element={<Reservations />} />
          <Route path="/PoliTutorBot/active-tutorings" element={<ActiveTutorings />} />
          <Route path="/PoliTutorBot/enabled-students" element={<EnabledStudents />} />
          <Route path="/PoliTutorBot/manage-tutors" element={<TutorManagement />} />
        </Routes>
      </div>
    </BrowserRouter>
    {/* <Footer/> */}
  </>
);
