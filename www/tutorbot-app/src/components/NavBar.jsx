import React from 'react';
import { useState } from 'react';

import Container from 'react-bootstrap/Container';
import Nav from 'react-bootstrap/Nav';
import Navbar from 'react-bootstrap/Navbar';
import { Link } from 'react-router-dom';

function Navigation()
{
  const [cards, setCards] = useState([false, false, false, false]);
  const selectCard = (index) =>
  {
    let temp = [false, false, false, false];
    temp[index] = true;
    setCards(temp);
  }

  const getClassFromUrl = (url) =>
  {
    if (url == "reservations" && window.location.hash == "")
    {
      return "navItemSelected";
    }
    return window.location.hash.includes(url) ? "navItemSelected" : "navItem";
  }

  return (
    <Navbar className="navBar" expand="lg">
      <Container>
        <Navbar.Toggle aria-controls="basic-navbar-nav" />
        <Navbar.Collapse id="basic-navbar-nav">
          <Nav className="navbarCollapse">
            <Nav.Item className={getClassFromUrl("reservations")} >
              <Nav.Link onClick={() => selectCard(0)} as={Link} to="reservations" className="text-light">
                Storico Prenotazioni
              </Nav.Link>
            </Nav.Item >
            <Nav.Item className={getClassFromUrl("active-tutorings")} >
              <Nav.Link onClick={() => selectCard(1)} as={Link} to="active-tutorings" className="text-light">
                Tutoraggi Attivi
              </Nav.Link>
            </Nav.Item >
            <Nav.Item className={getClassFromUrl("enabled-students")} >
              <Nav.Link onClick={() => selectCard(2)} as={Link} to="enabled-students" className="text-light">
                Matricole Abilitate
              </Nav.Link>
            </Nav.Item >
            <Nav.Item className={getClassFromUrl("manage-tutors")} >
              <Nav.Link onClick={() => selectCard(3)} as={Link} to="manage-tutors" className="text-light">
                Gestione Tutor
              </Nav.Link>
            </Nav.Item >
            <Nav.Item className={getClassFromUrl("manage-data")} >
              <Nav.Link onClick={() => selectCard(4)} as={Link} to="manage-data" className="text-light">
                Gestione Dati
              </Nav.Link>
            </Nav.Item >
          </Nav>
        </Navbar.Collapse>
      </Container>
    </Navbar>
  );
}

export default Navigation;

