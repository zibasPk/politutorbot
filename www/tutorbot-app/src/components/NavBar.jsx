import Container from 'react-bootstrap/Container';
import Nav from 'react-bootstrap/Nav';
import Navbar from 'react-bootstrap/Navbar';
import React from 'react';
import { Link } from 'react-router-dom';

function Navigation() {
  return (
    <Navbar className="navBar" expand="lg">
      <Container>
        <Navbar.Toggle aria-controls="basic-navbar-nav" />
        <Navbar.Collapse id="basic-navbar-nav">
          <Nav className="navbarCollapse">
            <Nav.Item className="navItem"><Nav.Link as={Link} to="/reservations" className="text-light">Storico Prenotazioni</Nav.Link></Nav.Item>
            <Nav.Item className="navItem"><Nav.Link as={Link} to="/active-tutorings" className="text-light">Tutoraggi Attivi</Nav.Link></Nav.Item>
            <Nav.Item className="navItem"><Nav.Link as={Link} to="/enabled-students" className="text-light">Matricole Abilitate</Nav.Link></Nav.Item>
            <Nav.Item className="navItem"><Nav.Link as={Link} to="/manage-tutors" className="text-light">Gestione Tutor</Nav.Link></Nav.Item>
          </Nav>
        </Navbar.Collapse>
      </Container>
    </Navbar>
  );
}

export default Navigation;

