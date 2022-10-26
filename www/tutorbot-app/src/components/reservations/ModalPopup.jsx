import React, { useState } from "react";
import Button from 'react-bootstrap/Button';
import Modal from 'react-bootstrap/Modal';

export default class ModalPopup extends React.Component {
  handleConfirm() {
    console.log(this.props.selectedList);
    this.props.handleVisibility();
  }

  render() {
    return (
      <Modal show={this.props.show} onHide={this.props.handleVisibility}>
        <Modal.Header closeButton>
          <Modal.Title>Modal heading</Modal.Title>
        </Modal.Header>
        <Modal.Body>Woohoo tutto quello che serve per mandare le Mail</Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={() => this.handleConfirm()}>
            Close
          </Button>
        </Modal.Footer>
      </Modal>
    )
  }
}

