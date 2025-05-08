import React from "react";
import './Cube.css';

export default function Cube() {
    return (
        <div className="cubeContainer">
            <div className="back side"></div>
            <div className="left side"></div>
            <div className="right side"></div>
            <div className="top side"></div>
            <div className="bottom side"></div>
            <div className="front side"></div>
        </div>
    );
}