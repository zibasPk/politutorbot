import React from "react";
import styles from './ActiveTutorings.module.css'

import { jsonToCSV } from 'react-papaparse'

import FileDownloadIcon from '@mui/icons-material/FileDownload';
import Form from 'react-bootstrap/Form';

export default class EndedTutoringsModal extends React.Component
{
  constructor(props)
  {
    super(props);
    this.state = {
      TutoringsList: props.selectedContent
    }
  }

  renderContent(props)
  {
    const rows = props.selectedList;

    const exportCsv = async () =>
    {
      const formattedArray = props.selectedList.map(({ Id, selected, IsOFA, ...keep }) =>
      {
        keep.StartDate = keep.StartDate.toLocaleString();
        keep.EndDate = keep.EndDate.toLocaleString();
        return Object.values(keep);
      }
      );

      const csv = jsonToCSV(
        {
          fields: [
            "Cod. Matr. Tutor",
            "Cognome Tutor",
            "Nome Tutor",
            "Codice Esame",
            "Cod. Matr. Studente",
            "Data Inizio",
            "Data Fine",
            "Durata in Ore",
          ],
          data: formattedArray
        }, { delimiter: ";" });

      var blob = new Blob([csv], { type: 'text/plain' });
      const element = document.createElement("a");
      element.href = URL.createObjectURL(blob);
      element.download = "tutoraggi-conclusi.csv";
      document.body.appendChild(element);
      element.click();
    }

    const renderBody = rows.map((tutoring) =>
    {
      return (
        <tr key={tutoring.Id}>
          <td>{tutoring.TutorCode}</td>
          <td>{tutoring.TutorSurname}</td>
          <td>{tutoring.TutorName}</td>
          <td>{tutoring.ExamCode}</td>
          <td>{tutoring.StudentCode}</td>
          <td>{tutoring.StartDate.toLocaleString()}</td>
          <td>{tutoring.EndDate.toLocaleString()}</td>
          <td>{tutoring.Duration}</td>
        </tr>
      );
    });

    if (rows.length !== 0)
    {
      return (
        <>
          <Form.Group controlId="formTutorCode" className="mb-3">
            <Form.Label>Download file CVS</Form.Label>
            <FileDownloadIcon className={styles.btnDownloadCvs} onClick={() => exportCsv()} />
          </Form.Group>
          <table className={styles.table}>
            <thead>
              <tr>
                <th scope="col">Cod. Matr. Tutor</th >
                <th scope="col">Cognome Tutor</th >
                <th scope="col">Nome Tutor</th >
                <th scope="col">Codice Esame</th >
                <th scope="col">Cod. Matr. Studente</th >
                <th scope="col">Data Inizio</th >
                <th scope="col">Data Fine</th >
                <th scope="col">Durata in Ore</th >
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

  render()
  {
    return (
      <>
        <this.renderContent selectedList={this.state.TutoringsList} />
      </>
    );
  }
}

