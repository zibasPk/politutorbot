import React from "react";

import styles from './Reservations.module.css';
import Table from "../utils/Table";
import ModalBody from "./ModalBody";
import { MakeCall } from "../../MakeCall";
import CircularProgress from '@mui/material/CircularProgress';


const ReservationsArray = [
  {
    id: 1,
    tutorNumber: 321321,
    tutorSurname: "Rossi",
    tutorName: "Mar",
    examCode: "09999",
    studentNumber: "938354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 2,
    tutorNumber: 321321,
    tutorSurname: "Morichetti",
    tutorName: "nario",
    examCode: "09999",
    studentNumber: "838354",
    timeStamp: "12-09-12 22:22:22",
    state: false,
    selected: false
  },
  {
    id: 3,
    tutorNumber: 321321,
    tutorSurname: "Nossi",
    tutorName: "Lario",
    examCode: "09999",
    studentNumber: "238354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 4,
    tutorNumber: 321321,
    tutorSurname: "Rossi",
    tutorName: "Mario",
    examCode: "09999",
    studentNumber: "938354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 5,
    tutorNumber: 321321,
    tutorSurname: "Rossi",
    tutorName: "Mario",
    examCode: "09999",
    studentNumber: "938354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 6,
    tutorNumber: 321321,
    tutorSurname: "Rossi",
    tutorName: "Mario",
    examCode: "09999",
    studentNumber: "938354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 7,
    tutorNumber: 321321,
    tutorSurname: "Morichetti",
    tutorName: "ario",
    examCode: "09999",
    studentNumber: "838354",
    timeStamp: "12-09-12 22:22:22",
    state: false,
    selected: false
  },
  {
    id: 8,
    tutorNumber: 321321,
    tutorSurname: "Nossi",
    tutorName: "Lario",
    examCode: "09999",
    studentNumber: "238354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 9,
    tutorNumber: 321321,
    tutorSurname: "Rossi",
    tutorName: "Mario",
    examCode: "09999",
    studentNumber: "938354",
    timeStamp: "10-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 10,
    tutorNumber: 321321,
    tutorSurname: "Rossi",
    tutorName: "Mario",
    examCode: "09999",
    studentNumber: "938354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 11,
    tutorNumber: 321321,
    tutorSurname: "Rossi",
    tutorName: "Mario",
    examCode: "09999",
    studentNumber: "938354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 12,
    tutorNumber: 321321,
    tutorSurname: "Morichetti",
    tutorName: "ario",
    examCode: "09999",
    studentNumber: "838354",
    timeStamp: "12-09-12 22:22:22",
    state: false,
    selected: false
  },
  {
    id: 13,
    tutorNumber: 321321,
    tutorSurname: "Nossi",
    tutorName: "Lario",
    examCode: "09999",
    studentNumber: "238354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 14,
    tutorNumber: 321321,
    tutorSurname: "Rossi",
    tutorName: "Mario",
    examCode: "09999",
    studentNumber: "938354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 15,
    tutorNumber: 321321,
    tutorSurname: "Rossi",
    tutorName: "Mario",
    examCode: "09999",
    studentNumber: "938354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 16,
    tutorNumber: 321321,
    tutorSurname: "Rossi",
    tutorName: "Mario",
    examCode: "09999",
    studentNumber: "938354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 17,
    tutorNumber: 321321,
    tutorSurname: "Morichetti",
    tutorName: "ario",
    examCode: "09999",
    studentNumber: "838354",
    timeStamp: "12-09-12 22:22:22",
    state: false,
    selected: false
  },
  {
    id: 18,
    tutorNumber: 321321,
    tutorSurname: "Nossi",
    tutorName: "Lario",
    examCode: "09999",
    studentNumber: "238354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 19,
    tutorNumber: 321321,
    tutorSurname: "Rossi",
    tutorName: "Mario",
    examCode: "09999",
    studentNumber: "938354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 20,
    tutorNumber: 321321,
    tutorSurname: "Rossi",
    tutorName: "Mario",
    examCode: "09999",
    studentNumber: "938354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 21,
    tutorNumber: 321321,
    tutorSurname: "Rossi",
    tutorName: "Mario",
    examCode: "09999",
    studentNumber: "938354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 22,
    tutorNumber: 321321,
    tutorSurname: "Ronco",
    tutorName: "ario",
    examCode: "09999",
    studentNumber: "838354",
    timeStamp: "12-09-12 22:22:22",
    state: false,
    selected: false
  },
  {
    id: 23,
    tutorNumber: 321321,
    tutorSurname: "Nossi",
    tutorName: "Lario",
    examCode: "09999",
    studentNumber: "238354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 24,
    tutorNumber: 111321,
    tutorSurname: "Rossi",
    tutorName: "Mario",
    examCode: "09999",
    studentNumber: "938354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 25,
    tutorNumber: 321321,
    tutorSurname: "Rossi",
    tutorName: "Mario",
    examCode: "09999",
    studentNumber: "938354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 26,
    tutorNumber: 999321,
    tutorSurname: "Rossi",
    tutorName: "Mario",
    examCode: "09999",
    studentNumber: "111354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
];

const Headers = {
  Id: "Prenotazione",
  Tutor: "Cod. Matr. Tutor",
  TutorSurname: "Cognome Tutor",
  TutorName: "Nome Tutor",
  Exam: "Codice Esame",
  Student: "Cod. Matr. Studente",
  ReservationTimestamp: "Data"
};

class Reservations extends React.Component {
  constructor(props) {
    super(props);
    this.state = {
      ProcessedReservations: [],
      PendingReservations: []
    };
  }

  // prepare object arrays for tables
  // let processedReservations = ReservationsArray.filter((x) => x.state);
  // let pendingReservations = ReservationsArray.filter((x) => !x.state);
  // processedReservations = processedReservations.map(({ state, ...key }) => key);
  // pendingReservations = pendingReservations.map(({ state, ...key }) => key);

  // fetch('https://api.npms.io/v2/search?q=react')
  //   .then(response => response.json())
  //   .then(data => console.log(data));



  // let reservatiore = MakeCall('GET', false, true, (response) => {

  // })
  componentDidMount() {
    MakeCall('GET','/reservations' , false, true, (reservations) => {
      reservations.map((elem) => {
        if (elem.Exam === null)
          elem.Exam = "OFA"
        elem.ReservationTimestamp = new Date(elem.ReservationTimestamp)
      }

      );
      let processedReservations = reservations.filter((x) => x.state)
        .map(({ state, ...key }) => key);
      let pendingReservations = reservations.filter((x) => !x.state)
        .map(({ state, ...key }) => key);
      this.setState({
        ProcessedReservations: processedReservations,
        PendingReservations: pendingReservations
      })
    })
  }

  render() {
    console.log(this.state.PendingReservations);
    return (
      <div className={styles.content}>
        <h1>Prenotazioni da Gestire</h1>

        {this.state.PendingReservations.length === 0 ? <CircularProgress /> :
          <>
            <Table headers={Headers} content={this.state.PendingReservations} hasChecks={true}
              modalProps={{
                modalContent: ModalBody,
                modalTitle: "Conferma prenotazione selezionate"
              }}
            />
          </>
        }
        <h1>Storico Prenotazioni</h1>
        {this.state.ProcessedReservations.length === 0 ? <CircularProgress /> :
          <Table headers={Headers} content={this.state.ProcessedReservations} hasChecks={false} />
        }
      </div>
    )
  }
}


export default Reservations;