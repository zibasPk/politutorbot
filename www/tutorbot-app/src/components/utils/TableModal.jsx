import React from "react";
import Button from 'react-bootstrap/Button';
import Modal from 'react-bootstrap/Modal';
import styles from './Table.module.css';


export default class TableModal extends React.Component {
  constructor(props) {
    super(props);
    this.state = {
      Body: props.modalContent,
      AltMessage: props.alt,
      Title: props.modalTitle,
      ConfirmAction: props.onConfirm
    }
  }

  handleConfirm() {
    if (this.state.ConfirmAction !== undefined)
      this.state.ConfirmAction();
    this.props.handleVisibility();
  }

  renderBody(props) {
    console.log(props.content);
    if (props.content !== undefined) {
      return (
        <>
          <props.content selectedContent={props.selectedContent} />
        </>
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
          <Modal.Title>{this.state.Title}</Modal.Title>
        </Modal.Header>
        <Modal.Body >
          <this.renderBody content={this.state.Body} alt={this.state.AltMessage} selectedContent={this.props.selectedContent} />
        </Modal.Body>
        <Modal.Footer>
          <Button variant="warning" onClick={() => this.handleConfirm()}>
            Annulla
          </Button>
          {
            this.state.ConfirmAction !== undefined ?
              <Button variant="secondary" onClick={() => this.handleConfirm()}>
                Conferma
              </Button>
              :
              <></>
          }
        </Modal.Footer>
      </Modal>
    )
  }
}
