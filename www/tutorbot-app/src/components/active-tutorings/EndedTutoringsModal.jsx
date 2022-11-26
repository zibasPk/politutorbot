import React from "react";
import styles from './ActiveTutorings.module.css'

import FileDownloadIcon from '@mui/icons-material/FileDownload';
import Form from 'react-bootstrap/Form';


export default class EndedTutoringsModal extends React.Component {
  constructor(props) {
    super(props);
    this.state = {
      TutoringsList: props.selectedContent
    }
  }

  renderContent(props) {
    const rows = props.selectedList;

    const renderBody = rows.map((tutoring) => {
      return (
        <tr key={tutoring.id}>
          <td>{tutoring.tutorNumber}</td>
          <td>{tutoring.tutorSurname}</td>
          <td>{tutoring.tutorName}</td>
          <td>{tutoring.studentNumber}</td>
          <td>{tutoring.examCode}</td>
          <td>{tutoring.start_date.toLocaleString()}</td>
          <td>{tutoring.end_date.toLocaleString()}</td>
        </tr>
      );
    });

    if (rows.length !== 0) {
      return (
        <>
          <Form.Group controlId="formTutorCode" className="mb-3">
            <Form.Label>Download file CVS</Form.Label>
            <FileDownloadIcon className={styles.btnDownloadCvs}/>
          </Form.Group>
          <table className={styles.table}>
            <thead>
              <tr>
                <th scope="col">Cod. Matr. Tutor</th >
                <th scope="col">Cognome Tutor</th >
                <th scope="col">Nome Tutor</th >
                <th scope="col">Cod. Matr. Studente</th >
                <th scope="col">Codice Esame</th >
                <th scope="col">Data Inizio</th >
                <th scope="col">Data Fine</th >
              </tr>
            </thead>
            <tbody>
              {renderBody}
            </tbody>
          </table>
        </>
      )
    }
    else
      return (<div>Nessun Tutoraggio Selezionato</div>);
  }

  render() {
    return (
      <>
        <this.renderContent selectedList={this.state.TutoringsList} />
      </>
    );
  }
}

