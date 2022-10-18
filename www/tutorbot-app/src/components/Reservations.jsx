import React from 'react';
import './Reservations.css';
import CheckBoxOutlineBlankIcon from '@mui/icons-material/CheckBoxOutlineBlank';
import ArrowDownwardIcon from '@mui/icons-material/ArrowDownward';
import ReservationTable from './ReservationTable';

function Reservations() {
  return (
    <>
      <h1>Reservations</h1>
      <ReservationTable name={"Prenotazione"} />
    </>
  );
}
export default Reservations;