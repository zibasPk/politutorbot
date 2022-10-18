import React from 'react';
import ReactDOM from 'react-dom/client';
import { BrowserRouter, Routes, Route } from "react-router-dom";
import './index.css';
import 'bootstrap/dist/css/bootstrap.min.css';

import Navigation from './components/NavBar';
import ActiveTutorings from './components/ActiveTutorings';
import EnabledStudents from './components/EnabledStudents';
import TutorManagement from './components/TutorManagement';
import Reservations from './components/Reservations';

const root = ReactDOM.createRoot(document.getElementById('root'));
root.render(
  <>
    <BrowserRouter>
      <Navigation />
      <Routes>
        <Route path="/reservations" element={<Reservations />} />
        <Route path="/active-tutorings" element={<ActiveTutorings />} />
        <Route path="/enabled-students" element={<EnabledStudents />} />
        <Route path="/manage-tutors" element={<TutorManagement />} />
      </Routes>
    </BrowserRouter>

  </>
);
