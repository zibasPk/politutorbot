import React from 'react';
import styles from "./ActiveTutorings.module.css"
import configData from "../../config/config.json"

import RefreshableComponent from '../Interfaces';
import Table from '../utils/Table';
import ActiveTutoringsModal from './ActiveTutoringsModal';
import EndedTutoringsModal from './EndedTutoringsModal';

import CircularProgress from '@mui/material/CircularProgress';

const ActiveHeaders = {
  TutorCode: "Cod. Matr. Tutor",
  TutorSurname: "Cognome Tutor",
  TutorName: "Nome Tutor",
  ExamCode: "Codice Esame",
  StudentCode: "Cod. Matr. Studente",
  StartDate: "Data Inizio",
}

const EndedHeaders = {
  TutorCode: "Cod. Matr. Tutor",
  TutorSurname: "Cognome Tutor",
  TutorName: "Nome Tutor",
  ExamCode: "Codice Esame",
  StudentCode: "Cod. Matr. Studente",
  StartDate: "Data Inizio",
  EndDate: "Data Fine",
  Duration: "Durata in Ore"
}

class ActiveTutorings extends RefreshableComponent {
  constructor(props) {
    super(props);
    this.state = {
      ActiveTutoringsArray: undefined,
      EndedTutoringsArray: undefined
    }
  }

  refreshData() {
    fetch(configData.botApiUrl + '/tutoring/active', {
      method: 'GET',
      headers: {
        'Authorization': 'Basic ' + btoa(configData.authCredentials),
      }
    }).then(resp => resp.json())
      .then((tutorings) => {
        tutorings.forEach((elem) => {
          if (elem.ExamCode === null)
            elem.ExamCode = "OFA"
          elem.StartDate = new Date(elem.StartDate);
        });
        this.setState({
          ActiveTutoringsArray: tutorings,
        })
      })

    fetch(configData.botApiUrl + '/tutoring/ended', {
      method: 'GET',
      headers: {
        'Authorization': 'Basic ' + btoa(configData.authCredentials),
      }
    }).then(resp => resp.json())
      .then((tutorings) => {
        tutorings.forEach((elem) => {
          if (elem.ExamCode === null)
            elem.ExamCode = "OFA"
          elem.StartDate = new Date(elem.StartDate);
          elem.EndDate = new Date(elem.EndDate);
        });

        this.setState({
          EndedTutoringsArray: tutorings,
        })
      })
  }

  render() {
    return (
      <>
        <div className={styles.content} >
          <h1>Tutoraggi Attivi</h1>
          {this.state.ActiveTutoringsArray === undefined ? <CircularProgress /> :
            <>
              <Table headers={ActiveHeaders}
                content={this.state.ActiveTutoringsArray} hasChecks={true}
                modalProps={{
                  modalContent: ActiveTutoringsModal,
                  modalTitle: "Concludi Tutoraggi selezionati",
                  onModalEvent: () => this.refreshData()
                }} />
            </>
          }
          <h1>Tutoraggi Conclusi</h1>
          {this.state.EndedTutoringsArray === undefined ? <CircularProgress /> :
            <>
              <Table headers={EndedHeaders}
                content={this.state.EndedTutoringsArray} hasChecks={true}
                modalProps={{
                  modalContent: EndedTutoringsModal,
                  modalTitle: "Esporta File CSV",
                }}
              />
            </>}
        </div>
      </>
    );
  }
}

export default ActiveTutorings;