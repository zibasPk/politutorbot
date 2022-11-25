import React from "react";
import Button from 'react-bootstrap/Button';
import Modal from 'react-bootstrap/Modal';
import styles from './Table.module.css';


export default class TableModal extends React.Component {
  constructor(props) {
    super(props);
    this.state = {
      Body: props.body,
      AltMessage: props.alt
    }
  }

  handleConfirm() {
    this.props.handleVisibility();
  }

  renderBody(props) {
    if (props.body !== undefined) {
      return (
        props.body
      )
    }
    else
      return (<div>{props.alt}</div>);
  }

  render() {
    return (
      <Modal show={this.props.show}
        onHide={this.props.handleVisibility}
        backdrop="static"
        dialogClassName={styles.myModal}
      >
        <Modal.Header closeButton>
          <Modal.Title>Conferma prenotazione selezionate</Modal.Title>
        </Modal.Header>
        <Modal.Body >
          <this.renderBody body={this.state.Body} alt={this.state.AltMessage} />
        </Modal.Body>
        <Modal.Footer>
          <Button variant="warning" onClick={() => this.handleConfirm()}>
            Annulla
          </Button>
        </Modal.Footer>
      </Modal>
    )
  }
}
