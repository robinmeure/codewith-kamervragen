import { Button, makeStyles,
    tokens } from '@fluentui/react-components';
import { Message } from './Message';
import { useEffect, useRef } from 'react';
import { IChatMessage } from '../../models/ChatMessage';
import { SelectedQAPair } from '../Search/QuestionAnswerList';

const useClasses = makeStyles({
    scrollContainer: {
        flex: 1,
        heigth: '100%',
        display: 'flex',
        overflow: 'scroll',
        overflowX: 'hidden',
        flexDirection: 'column',
        '&::-webkit-scrollbar': {
            display: 'none'
        },
    },
    messageContainer: {
        width: '70%',
        margin: 'auto',
        height: 'calc(100vh - 60px)',
    },
    chatEmptyState: {
    },
    examplesNavList: {
        listStyle: 'none',
        paddingLeft: '1rem',
        paddingRight: '1rem',
        display: 'flex',
        flexWrap: 'wrap',
        gap: '0.625rem',
        flex: 1,
        justifyContent: 'center',
      },
      example: {
        wordBreak: 'break-word',
        backgroundColor: tokens.colorNeutralBackground1Hover,
        borderRadius: '0.5rem',
        display: 'flex',
        flexDirection: 'column',
        marginBottom: '0.3125rem',
        cursor: 'pointer',
      },
      exampleTitle: {
        // Add styles if needed
      },
      exampleText: {
        margin: 0,
        fontSize: '1.25rem',
        width: '25rem',
        padding: '0.5rem',
        minHeight: '4.5rem',
      },
      followUpContainer: {
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'flex-end',
        marginTop: tokens.spacingVerticalS,
        marginLeft: 'auto',
        marginRight: 'auto',
        maxWidth: '80%',
    },
    followUpButton: {
        marginTop: tokens.spacingVerticalXS
    },
});

type messageListProps = {
    messages: IChatMessage[];
    loading: boolean;
    onFollowUp: (question: string) => void;
    onQASelected: (selectedPairs: SelectedQAPair[]) => void;
    
    selectedChatId: string | undefined; // Add this line
}

export function MessageList({ messages, loading, selectedChatId, onFollowUp, onQASelected }: messageListProps) {

    const classes = useClasses();
    const containerRef = useRef<HTMLDivElement>(null);
   
    useEffect(() => {
        if (containerRef.current ) {
            containerRef.current.scrollTo({
                top: containerRef.current.scrollTop = containerRef.current.scrollHeight,
                behavior: 'smooth',
            });
        }
    }, [messages]);

    return (
        <div ref={containerRef} className={classes.scrollContainer}>
            {messages && messages.length <= 0 && (
                <div className={classes.followUpContainer}>
                    <Button key="1" className={classes.followUpButton} onClick={() => onFollowUp("Kan je gekozen vragen samenvatten en beantwoorden?")}>
                        Kan je gekozen vragen samenvatten en beantwoorden?
                    </Button>
                    <Button key="2" className={classes.followUpButton} onClick={() => onFollowUp("Wat is het sentiment achter de vragen die gesteld zijn?")}>
                        Wat is het sentiment achter de vragen die gesteld zijn?
                    </Button>
                </div>
            )}

            <div className={classes.messageContainer}>
                {messages.map((message) => (
                    <Message key={message.id} message={message} onFollowUp={onFollowUp} selectedChatId={selectedChatId} onQASelected={onQASelected} />
                ))}
                {loading && <div>Loading...</div>}
            </div>
        </div>

    );
};