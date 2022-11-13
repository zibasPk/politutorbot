import Container from 'react-bootstrap/Container';
import Nav from 'react-bootstrap/Nav';
import Navbar from 'react-bootstrap/Navbar';
import React from 'react';
import { Link } from 'react-router-dom';

function Navigation() {
  return (
    <Navbar bg="light" expand="lg">
      <Container>
        <Navbar.Toggle aria-controls="basic-navbar-nav" />
        <Navbar.Collapse id="basic-navbar-nav">
          <Nav className="me-auto">
            <Nav.Item><Nav.Link as={Link} to="/reservations">Storico Prenotazioni</Nav.Link></Nav.Item>
            <Nav.Item><Nav.Link as={Link} to="/active-tutorings">Tutoraggi Attivi</Nav.Link></Nav.Item>
            <Nav.Item><Nav.Link as={Link} to="/enabled-students">Matricole Abilitate</Nav.Link></Nav.Item>
            <Nav.Item><Nav.Link as={Link} to="/manage-tutors">Gestione Tutor</Nav.Link></Nav.Item>
          </Nav>
        </Navbar.Collapse>
      </Container>
    </Navbar>
  );
}

export default Navigation;

