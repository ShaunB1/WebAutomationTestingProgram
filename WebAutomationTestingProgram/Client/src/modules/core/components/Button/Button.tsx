interface ButtonProps {
    content: string;
    onClick?: () => void;
}

function Button(props: ButtonProps) {
    return (
        <>
            <button onClick={props.onClick}>{props.content}</button>
        </>
    );
}

export default Button;